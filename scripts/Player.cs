using Godot;
using OpenWorldSurvival.scripts.signals;

namespace OpenWorldSurvival.scripts;

public partial class Player : CharacterBody3D
{
    private const float JumpVelocity = 6f;
    private const float CrouchingDepth = -.5f;
    private const float CrouchingSpeed = 3f;
    private const float WalkingSpeed = 5f;
    private const float SprintingSpeed = 8f;
    private const float TerminalVelocity = -60.0f;
    private const float MouseSensitivity = 0.2f;
    private const float LerpSpeed = 10.0f;
    private const float HeadHeight = 1.5f;
    private bool _canDoubleJump;
    private Crafting _crafting;
    private float _currentSpeed = 5.0f;
    private Vector3 _direction;

    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    private Node3D _head;
    private bool _isJumping;
    private Crafting _smelting;

    public override void _Ready()
    {
        _head = GetNode<Node3D>("Head");
        _crafting = GetNode<Crafting>("Crafting");
        _smelting = GetNode<Crafting>("Smelting");
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("quit"))
            GetTree().Quit();

        // Close crafting on mouse click (excluding UI interaction)
        if (_crafting.IsCraftingWindowOpen() &&
            @event is InputEventMouseButton { Pressed: true })
            GlobalSignals.Instance.EmitOnCloseCraftingMenu(_crafting.CraftingTypeName);
        if (_smelting.IsCraftingWindowOpen() &&
            @event is InputEventMouseButton { Pressed: true })
            GlobalSignals.Instance.EmitOnCloseCraftingMenu(_smelting.CraftingTypeName);

        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            RotateY(Mathf.DegToRad(-mouseMotion.Relative.X * MouseSensitivity));
            _head.RotateX(Mathf.DegToRad(-mouseMotion.Relative.Y * MouseSensitivity));
            _head.Rotation = _head.Rotation.SetX(Mathf.DegToRad(Mathf.Clamp(_head.RotationDegrees.X, -80, 80)));
        }

        if (@event is InputEventMouseButton)
            Input.MouseMode = Input.MouseModeEnum.Captured;
        else if (Input.IsActionPressed("cancel"))
            Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
            velocity.Y -= _gravity * (float)delta;

        // Handle the double jump.
        if (IsOnFloor())
        {
            _isJumping = false;
            _canDoubleJump = true; // Reset double jump when on floor
            if (Input.IsActionJustPressed("jump"))
            {
                velocity.Y = JumpVelocity;
                _isJumping = true;
            }
        }
        else if (_isJumping && _canDoubleJump && Input.IsActionJustPressed("jump"))
        {
            velocity.Y = JumpVelocity;
            _canDoubleJump = false; // Use up the double jump
            _isJumping = true;
        }

        // Limit fall speed
        if (velocity.Y < TerminalVelocity) velocity.Y = TerminalVelocity;

        // Handle character's crouching/walking/sprinting state.
        if (Input.IsActionPressed("crouch"))
        {
            _currentSpeed = CrouchingSpeed;
            _head.Position = _head.Position.SetY(Mathf.Lerp(_head.Position.Y, HeadHeight + CrouchingDepth,
                (float)(delta * LerpSpeed)));
        }
        else
        {
            _head.Position = _head.Position.SetY(Mathf.Lerp(_head.Position.Y, HeadHeight,
                (float)(delta * LerpSpeed)));
            _currentSpeed = Input.IsActionPressed("sprint") ? SprintingSpeed : WalkingSpeed;
        }

        // Get the input direction and handle the movement/deceleration.
        var inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");

        if (inputDir != Vector2.Zero && _crafting.IsCraftingWindowOpen())
            GlobalSignals.Instance.EmitOnCloseCraftingMenu(_crafting.CraftingTypeName);
        if (inputDir != Vector2.Zero && _smelting.IsCraftingWindowOpen())
            GlobalSignals.Instance.EmitOnCloseCraftingMenu(_smelting.CraftingTypeName);

        _direction = _direction.Lerp((Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized(),
            (float)(delta * LerpSpeed));

        // Move the character in the direction of the input at the right speed.
        if (_direction != Vector3.Zero)
        {
            velocity.X = _direction.X * _currentSpeed;
            velocity.Z = _direction.Z * _currentSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, _currentSpeed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, _currentSpeed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}