using System.Text;
using Godot;
using OpenWorldSurvival.scripts;
using OpenWorldSurvival.scripts.signals;
using OpenWorldSurvival.scripts.utils;

namespace OpenWorldSurvival.FirstPersonController;

public partial class FirstPersonController : CharacterBody3D
{
    private const float DashCooldown = 5000;
    private const float DashDuration = 1.5f;
    private const bool InAirMomentum = true;
    private const float JumpVelocity = 4f;
    private const float MouseSensitivity = 0.005f;
    private const float SlideDuration = 1f;
    private const float Acceleration = 5.0f;
    private const float BaseSpeed = 4.0f;
    private const float SlideSpeed = 8.0f;
    private const float SprintSpeed = 6.0f;
    private const float DashSpeed = 10.0f;
    private const float CrouchSpeed = 2.0f;
    private const float LerpBackTime = 0.25f;


    private Camera3D _camera;
    private ShapeCast3D _ceilingDetection;
    private CollisionShape3D _collisionCrouching;
    private CollisionShape3D _collisionStanding;
    private Crafting _crafting;
    private AnimationPlayer _crouchAnimation;

    private float _currentSpeed;
    private bool _dashOnCooldown;
    private float _dashTimer;
    private DebugPanel _debugPanel;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    private Node3D _head;
    private AnimationPlayer _headBobAnimationPlayer;
    private Vector3 _initialSlideDirection = Vector3.Zero;

    private bool _isDashing;
    private bool _isSliding;
    private AnimationPlayer _jumpAnimationPlayer;
    private bool _lowCeiling; // This is for when the ceiling is too low and the player needs to crouch.
    private Reticle _reticle;
    private float _slideTimer;
    private Crafting _smelting;
    private float _speed;

    // States: normal, crouching, sprinting, dashing, sliding
    private string _state = "normal";

    private ColorRect _superSonicEffect;
    private Control _userInterface;
    private bool _wasOnFloor = true;

    public override void _Ready()
    {
        _speed = BaseSpeed;
        _userInterface = GetNode<Control>("UserInterface");
        _debugPanel = _userInterface.GetNode<DebugPanel>("DebugPanel");
        _head = GetNode<Node3D>("Head");
        _jumpAnimationPlayer = GetNode<AnimationPlayer>("Head/JumpAnimation");
        _headBobAnimationPlayer = GetNode<AnimationPlayer>("Head/HeadBobAnimation");
        _crouchAnimation = GetNode<AnimationPlayer>("CrouchAnimation");
        _camera = GetNode<Camera3D>("Head/Camera");
        _ceilingDetection = GetNode<ShapeCast3D>("CrouchCeilingDetection");
        _collisionStanding = GetNode<CollisionShape3D>("CollisionStanding");
        _collisionCrouching = GetNode<CollisionShape3D>("CollisionCrouching");
        _crafting = GetNode<Crafting>("Crafting");
        _smelting = GetNode<Crafting>("Smelting");
        _superSonicEffect = GetNode<ColorRect>("UserInterface/SuperSonicEffect");
        _superSonicEffect.MouseFilter = Control.MouseFilterEnum.Ignore;

        Input.MouseMode = Input.MouseModeEnum.Captured;

        _reticle = GetNode<Reticle>("UserInterface/Reticle");

        _headBobAnimationPlayer.Play("RESET");
        _jumpAnimationPlayer.Play("RESET");
        _crouchAnimation.Play("RESET");
    }

    public Transform3D GetHeadTransform()
    {
        return _head.GlobalTransform;
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentSpeed = Vector3.Zero.DistanceTo(GetRealVelocity());
        _superSonicEffect.Visible = _currentSpeed > 9;

        // Use StringBuilder to minimize string allocations
        var debugInfo = new StringBuilder();
        debugInfo.Append($"Speed: {_currentSpeed:0.000}");
        _debugPanel.AddProperty("Speed", debugInfo.ToString(), 1);

        debugInfo.Clear();
        debugInfo.Append($"Target Speed: {_speed}");
        _debugPanel.AddProperty("Target Speed", debugInfo.ToString(), 2);

        var cv = GetRealVelocity();
        debugInfo.Clear();
        debugInfo.AppendFormat("Velocity: X: {0:0.000} Y: {0:0.000} Z: {0:0.000}", cv.X, cv.Y, cv.Z);
        _debugPanel.AddProperty("Velocity", debugInfo.ToString(), 3);

        HandleGravityAndJumping(delta);
        var inputDir = Input.GetVector(InputAction.Left, InputAction.Right,
            InputAction.Forward, InputAction.Backward);
        HandleMovement(delta, inputDir);

        if (inputDir != Vector2.Zero && _crafting.IsCraftingWindowOpen())
            GlobalSignals.Instance.EmitOnCloseCraftingMenu(_crafting.CraftingTypeName);
        if (inputDir != Vector2.Zero && _smelting.IsCraftingWindowOpen())
            GlobalSignals.Instance.EmitOnCloseCraftingMenu(_smelting.CraftingTypeName);

        _lowCeiling = _ceilingDetection.IsColliding();
        HandleState(inputDir != Vector2.Zero);
        UpdateCameraFov();
        HeadBobAnimation(inputDir != Vector2.Zero);

        if (_isDashing)
        {
            _dashTimer -= (float)delta;
            if (_dashTimer <= 0)
            {
                _isDashing = false;
                _speed = BaseSpeed;
                EnterNormalState();
            }
        }
        else if (_dashOnCooldown)
        {
            _dashTimer -= 1;
            if (_dashTimer <= 0)
            {
                _dashOnCooldown = false;

                if (Input.IsActionPressed(InputAction.Sprint))
                    EnterSprintState();
                else
                    EnterNormalState();
            }

            {
                var slideVelocity = _initialSlideDirection * _speed;
                Velocity = Velocity.SetXz(slideVelocity.X, slideVelocity.Z);
            }
        }

        if (_isSliding)
        {
            _slideTimer -= (float)delta;
            if (_slideTimer <= 0)
            {
                _isSliding = false;
                if (_lowCeiling)
                {
                    EnterCrouchState();
                    return;
                }

                if (Input.IsActionPressed(InputAction.Sprint))
                    EnterSprintState();
                else
                    EnterNormalState();
            }
            else
            {
                var slideVelocity = _initialSlideDirection * _speed;
                Velocity = Velocity.SetXz(slideVelocity.X, slideVelocity.Z);
            }
        }

        if (!_wasOnFloor && IsOnFloor()) // Just landed
            _jumpAnimationPlayer.Play(GD.Randi() % 2 == 1 ? "land_left" : "land_right");
        _wasOnFloor = IsOnFloor(); // This must always be at the end of physics_process
    }

    private void HandleGravityAndJumping(double delta)
    {
        var currentVelocity = Velocity;
        if (!IsOnFloor())
            currentVelocity.Y -= (float)(_gravity * delta);

        if (Input.IsActionPressed(InputAction.Jump))
            if (IsOnFloor() && !_lowCeiling)
            {
                _jumpAnimationPlayer?.Play("jump");
                currentVelocity.Y += _state == "sliding" ? JumpVelocity * 1.5f : JumpVelocity;
            }

        Velocity = currentVelocity;
    }

    private void HandleMovement(double delta, Vector2 inputDir)
    {
        var direction2D = inputDir.Rotated(-_head.Rotation.Y);
        var direction = new Vector3(direction2D.X, 0, direction2D.Y);
        direction = direction.Normalized();
        MoveAndSlide();

        if (InAirMomentum && !IsOnFloor()) return;
        var currentVelocity = Vector3.Zero;
        currentVelocity.X = Mathf.Lerp(Velocity.X, direction.X * _speed, (float)(Acceleration * delta));
        currentVelocity.Z = Mathf.Lerp(Velocity.Z, direction.Z * _speed, (float)(Acceleration * delta));
        Velocity = currentVelocity;
    }

    private void HandleState(bool moving)
    {
        if (_isDashing || _isSliding) return; // Do not change state if dashing or sliding

        if (Input.IsActionPressed(InputAction.Sprint) && _state != "crouching")
            if (moving)
                EnterSprintState();

        if (Input.IsActionJustReleased(InputAction.Sprint) && _state != "crouching") EnterNormalState();

        if (Input.IsActionJustPressed(InputAction.Dash) && _state == "sprinting")
            if (!_dashOnCooldown)
                EnterDashState();

        if (Input.IsActionPressed(InputAction.Crouch) && _state != "sprinting")
        {
            if (_state != "crouching") EnterCrouchState();
        }
        else if (_state == "crouching" && !_lowCeiling)
        {
            EnterNormalState();
        }

        if (_state == "sprinting" && Input.IsActionJustPressed(InputAction.Crouch)) EnterSlideState();
    }

    private void EnterDashState()
    {
        if (_dashOnCooldown) return;

        _isDashing = true;
        _dashTimer = DashDuration;
        _dashOnCooldown = true;
        _state = "dash";
        _speed = DashSpeed;
        _initialSlideDirection = Velocity.Normalized();
    }

    private void EnterSlideState()
    {
        _isSliding = true;
        _collisionStanding.Disabled = true;
        _collisionCrouching.Disabled = false;
        _slideTimer = SlideDuration;
        _speed = SlideSpeed;
        _state = "sliding";
        _initialSlideDirection = Velocity.Normalized();
        _crouchAnimation.Play("crouch");
    }

    private void EnterNormalState()
    {
        var previousState = _state;
        if (previousState is "crouching" or "sliding")
        {
            _collisionStanding.Disabled = false;
            _collisionCrouching.Disabled = true;
            _crouchAnimation.PlayBackwards("crouch");
        }

        _state = "normal";
        _speed = BaseSpeed;
    }

    private void EnterSprintState()
    {
        var previousState = _state;
        if (previousState is "crouching" or "sliding")
        {
            _collisionStanding.Disabled = false;
            _collisionCrouching.Disabled = true;
            _crouchAnimation.PlayBackwards("crouch");
        }

        _state = "sprinting";
        _speed = SprintSpeed;
    }

    private void EnterCrouchState()
    {
        var previousState = _state;
        _collisionStanding.Disabled = true;
        _collisionCrouching.Disabled = false;
        _state = "crouching";
        _speed = CrouchSpeed;
        if (previousState != "sliding")
            _crouchAnimation.Play("crouch");
    }

    private void UpdateCameraFov()
    {
        _camera.Fov = Mathf.Lerp(_camera.Fov, _state == "sprinting" ? 80 : 85, 0.3f);
    }

    private void HeadBobAnimation(bool moving)
    {
        if (moving && IsOnFloor())
        {
            var useHeadBobAnimation = _state is "normal" or "crouching" ? "walk" : "sprint";
            var wasPlaying = _headBobAnimationPlayer.CurrentAnimation == useHeadBobAnimation;

            _headBobAnimationPlayer.Play(useHeadBobAnimation, 0.25f);
            _headBobAnimationPlayer.SpeedScale = _currentSpeed / BaseSpeed * 1.75f;
            if (!wasPlaying) _headBobAnimationPlayer.Seek(GD.Randi() % 2);
        }
        else
        {
            _headBobAnimationPlayer.Play("RESET", 0.25);
            _headBobAnimationPlayer.SpeedScale = 1;
        }
    }

    public override void _Process(double delta)
    {
        _debugPanel.AddProperty("FPS", $"{Performance.GetMonitor(Performance.Monitor.TimeFps)}", 0);
        _debugPanel.AddProperty("state", $"{_state}" + (!IsOnFloor() ? " in the air" : ""), 4);
        if (Input.IsActionJustPressed(InputAction.Pause))
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                ? Input.MouseModeEnum.Visible
                : Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed(InputAction.Quit))
            GetTree().Quit();

        // Close crafting on mouse click (excluding UI interaction)
        if (_crafting.IsCraftingWindowOpen() &&
            @event is InputEventMouseButton { Pressed: true })
            GlobalSignals.Instance.EmitOnCloseCraftingMenu(_crafting.CraftingTypeName);
        if (_smelting.IsCraftingWindowOpen() &&
            @event is InputEventMouseButton { Pressed: true })
            GlobalSignals.Instance.EmitOnCloseCraftingMenu(_smelting.CraftingTypeName);

        if (@event is InputEventMouseButton)
            Input.MouseMode = Input.MouseModeEnum.Captured;
        else if (Input.IsActionPressed("cancel"))
            Input.MouseMode = Input.MouseModeEnum.Visible;

        HandleJoystickInput();

        if (@event is not InputEventMouseMotion motion || Input.MouseMode != Input.MouseModeEnum.Captured) return;
        var currentRotation = _head.Rotation;
        currentRotation.Y -= motion.Relative.X * MouseSensitivity;
        currentRotation.X -= motion.Relative.Y * MouseSensitivity;

        // Clamp the vertical rotation
        currentRotation.X = Mathf.Clamp(currentRotation.X, Mathf.DegToRad(-89), Mathf.DegToRad(89));

        _head.Rotation = currentRotation;
    }

    private static void HandleJoystickInput()
    {
        const int joyIndex = 0; // Change this to the appropriate joystick index

        for (var axisIndex = 0; axisIndex < (int)JoyAxis.Max; axisIndex++)
            // Ensure the axis index is within bounds
            if (axisIndex is >= 0 and < (int)JoyAxis.Max)
            {
                var axisValue = Input.GetJoyAxis(joyIndex, JoyAxis.LeftX + axisIndex);
            }
            else
            {
                GD.PrintErr($"Error: Axis index {axisIndex} is out of bounds. Valid range is 0 to {JoyAxis.Max - 1}");
            }
    }
}