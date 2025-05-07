using System.Text;
using Godot;
using OpenWorldSurvival.engine.godot;
using OpenWorldSurvival.game.globals.constants;
using OpenWorldSurvival.game.globals.signals;
using OpenWorldSurvival.game.scripts.systems.crafting;
using OpenWorldSurvival.game.scripts.systems.inventory;

namespace OpenWorldSurvival.game.scripts.actors.player;

public partial class FirstPersonController : CharacterBody3D
{
    private const float DashDuration = 1.5f;
    private const bool InAirMomentum = true;
    private const float JumpVelocity = 4f;
    private const float MouseSensitivity = 0.005f;
    private const float SlideDuration = 1f;
    private const float Acceleration = 15.0f;
    private const float BaseSpeed = 4.0f;
    private const float SlideSpeed = 8.0f;
    private const float SlideJumpSpeed = 10f;
    private const float SprintSpeed = 6.0f;
    private const float DashSpeed = 12f;
    private const float CrouchSpeed = 2.0f;
    private const float FreeLookLerpTime = 0.2f;
    private const float RollLerpDuration = .2f;
    private readonly float _freeLookYLimit = Mathf.DegToRad(125);
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
    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    private Node3D _head;
    private AnimationPlayer _headBobAnimationPlayer;
    private bool _ignoreNextMouseMotion;
    private Inventory _inventory;
    private bool _isDashing;
    private bool _isLerpingRoll;
    private bool _isSlideJumping;
    private bool _isSliding;
    private AnimationPlayer _jumpAnimationPlayer;
    private float _lerpRollProgress;
    private Vector3 _lockDirection = Vector3.Zero;
    private bool _lowCeiling;
    private Reticle _reticle;
    private float _slideTimer;
    private Crafting _smelting;
    private float _speed;
    private float _startRoll;
    private string _state = "normal";
    private ColorRect _superSonicEffect;
    private float _targetRoll;
    private Control _userInterface;
    private bool _wasOnFloor = true;

    public override void _Ready()
    {
        _speed = BaseSpeed;
        _ignoreNextMouseMotion = false;
        _userInterface = GetNode<Control>("UserInterface");
        _debugPanel = _userInterface.GetNode<DebugPanel>("DebugPanel");
        _head = GetNode<Node3D>("Head");
        _jumpAnimationPlayer = GetNode<AnimationPlayer>("Head/JumpAnimation");
        _headBobAnimationPlayer = GetNode<AnimationPlayer>("Head/HeadBobAnimation");
        _crouchAnimation = GetNode<AnimationPlayer>("CrouchAnimation");
        _inventory = GetNode<Inventory>("Inventory");
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

        var debugInfo = new StringBuilder();
        debugInfo.Append($"Speed: {_currentSpeed:0.000}");
        _debugPanel.AddProperty("Speed", debugInfo.ToString(), 1);

        debugInfo.Clear();
        debugInfo.Append($"Target Speed: {_speed}");
        _debugPanel.AddProperty("Target Speed", debugInfo.ToString(), 2);

        var cv = GetRealVelocity();
        debugInfo.Clear();
        debugInfo.AppendFormat("X: {0:0.000} Y: {0:0.000} Z: {0:0.000}", cv.X, cv.Y, cv.Z);
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

        _superSonicEffect.Visible = _isDashing;
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
            else
            {
                var slideVelocity = _lockDirection * _speed;
                Velocity = Velocity.SetXz(slideVelocity.X, slideVelocity.Z);
            }
        }

        if (_isSliding)
        {
            _slideTimer -= (float)delta;
            if (_slideTimer <= 0)
            {
                _isSliding = false;
                // Extract yaw (Y-axis) and pitch (X-axis) from the camera's global rotation
                var cameraGlobalBasis = _camera.GlobalTransform.Basis;
                var yaw = Mathf.Atan2(cameraGlobalBasis.Z.X, cameraGlobalBasis.Z.Z);
                var forward = cameraGlobalBasis.Z;
                var pitch = Mathf.Asin(-forward.Y);
                pitch = Mathf.Clamp(pitch, Mathf.DegToRad(-89), Mathf.DegToRad(89));

                // Set pitch and yaw immediately, start lerping roll to 0
                _startRoll = _camera.GlobalRotation.Z;
                _head.GlobalRotation = new Vector3(pitch, yaw, _startRoll);
                _targetRoll = 0f;
                _lerpRollProgress = 0f;
                _isLerpingRoll = true;

                _camera.Rotation = Vector3.Zero;
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

            if (_isSlideJumping)
            {
                var slideVelocity = _lockDirection * _speed;
                Velocity = Velocity.SetXz(slideVelocity.X, slideVelocity.Z);
            }
        }

        // Handle roll interpolation
        if (_isLerpingRoll)
        {
            _lerpRollProgress += (float)delta / RollLerpDuration;
            if (_lerpRollProgress >= 1f)
            {
                _lerpRollProgress = 1f;
                _isLerpingRoll = false;
            }

            var lerpedRoll = Mathf.LerpAngle(_startRoll, _targetRoll, _lerpRollProgress);
            _head.GlobalRotation = new Vector3(_head.GlobalRotation.X, _head.GlobalRotation.Y, lerpedRoll);
        }

        // Handle landing after the slide jump
        if (!_wasOnFloor && IsOnFloor() && _isSlideJumping)
        {
            _isSlideJumping = false;
            _isSliding = false;
            if (Input.IsActionPressed(InputAction.Sprint))
                EnterSprintState();
            else
                EnterNormalState();
        }

        if (!_wasOnFloor && IsOnFloor())
            _jumpAnimationPlayer.Play(GD.Randi() % 2 == 1 ? "land_left" : "land_right");
        _wasOnFloor = IsOnFloor();
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
                if (_state == "sliding")
                {
                    _isSlideJumping = true;
                    _speed = SlideJumpSpeed;
                    EnterSprintState();
                }
            }

        Velocity = currentVelocity;
    }

    private void HandleMovement(double delta, Vector2 inputDir)
    {
        var direction2D = inputDir.Rotated(-_head.Rotation.Y);
        var direction = new Vector3(direction2D.X, 0, direction2D.Y);
        direction = direction.Normalized();
        MoveAndSlide();

        // Apply dash velocity during dash, ignoring input
        if (_isDashing)
        {
            var dashVelocity = _lockDirection * _speed;
            Velocity = Velocity.SetXz(dashVelocity.X, dashVelocity.Z);
            return;
        }

        // Skip normal movement updates if in slide jump
        if (_isSlideJumping)
        {
            var slideVelocity = _lockDirection * _speed;
            Velocity = Velocity.SetXz(slideVelocity.X, slideVelocity.Z);
            return;
        }

        if (InAirMomentum && !IsOnFloor()) return;
        var currentVelocity = Vector3.Zero;
        currentVelocity.X = Mathf.Lerp(Velocity.X, direction.X * _speed, (float)(Acceleration * delta));
        currentVelocity.Z = Mathf.Lerp(Velocity.Z, direction.Z * _speed, (float)(Acceleration * delta));
        Velocity = currentVelocity;
    }

    private void HandleState(bool moving)
    {
        if (_isDashing || _isSliding || _isSlideJumping) return;

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
        // Set dash direction based on the current velocity or forward direction
        _lockDirection = Velocity.Normalized();
        if (_lockDirection == Vector3.Zero)
            _lockDirection = -_head.GlobalTransform.Basis.Z.Normalized();
    }

    private void EnterSlideState()
    {
        _isSliding = true;
        _collisionStanding.Disabled = true;
        _collisionCrouching.Disabled = false;
        _slideTimer = SlideDuration;
        _speed = SlideSpeed;
        _state = "sliding";
        _lockDirection = Velocity.Normalized();
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
        if (!_isSlideJumping) _speed = BaseSpeed;
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
        if (!_isSlideJumping) _speed = SprintSpeed;
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

        if (!Input.IsActionPressed(InputAction.FreeLook) && !_isSliding)
        {
            var t = (float)delta / FreeLookLerpTime;
            _camera.Rotation = _camera.Rotation.Lerp(Vector3.Zero, t);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed(InputAction.Quit))
            GetTree().Quit();

        if (_crafting.IsCraftingWindowOpen() &&
            @event is InputEventMouseButton { Pressed: true })
            GlobalSignals.Instance.EmitOnCloseCraftingMenu(_crafting.CraftingTypeName);
        if (_smelting.IsCraftingWindowOpen() &&
            @event is InputEventMouseButton { Pressed: true })
            GlobalSignals.Instance.EmitOnCloseCraftingMenu(_smelting.CraftingTypeName);

        if (@event is InputEventMouseButton)
        {
            if (Input.MouseMode != Input.MouseModeEnum.Captured)
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
                _ignoreNextMouseMotion = true;
            }
        }
        else if (Input.IsActionPressed("cancel"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        HandleJoystickInput();

        if (@event is not InputEventMouseMotion motion || Input.MouseMode != Input.MouseModeEnum.Captured) return;

        if (_ignoreNextMouseMotion)
        {
            _ignoreNextMouseMotion = false;
            return;
        }

        if (_isDashing) return;
        if (Input.IsActionPressed(InputAction.FreeLook) || (_isSliding && IsOnFloor()))
        {
            var cameraRotation = _camera.Rotation;
            cameraRotation.Y -= motion.Relative.X * MouseSensitivity;
            cameraRotation.X -= motion.Relative.Y * MouseSensitivity;
            cameraRotation.Y = Mathf.Clamp(cameraRotation.Y, -_freeLookYLimit, _freeLookYLimit);
            _camera.Rotation = cameraRotation;
        }
        else
        {
            var headRotation = _head.Rotation;
            headRotation.Y -= motion.Relative.X * MouseSensitivity;
            headRotation.X -= motion.Relative.Y * MouseSensitivity;
            headRotation.X = Mathf.Clamp(headRotation.X, Mathf.DegToRad(-89), Mathf.DegToRad(89));
            _head.Rotation = headRotation;
        }
    }

    public bool IsInAttackableState()
    {
        var isNormalOrCrouching = _state is "normal" or "crouching" or "sliding" || !IsOnFloor();
        var isUiOpen = _crafting.IsCraftingWindowOpen() || _smelting.IsCraftingWindowOpen() || _inventory.IsOpen();
        return isNormalOrCrouching && !isUiOpen;
    }

    private static void HandleJoystickInput()
    {
        const int joyIndex = 0;

        for (var axisIndex = 0; axisIndex < (int)JoyAxis.Max; axisIndex++)
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