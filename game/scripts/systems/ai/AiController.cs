using Godot;

namespace OpenWorldSurvival.game.scripts.systems.ai;

public partial class AiController : CharacterBody3D
{
    private NavigationAgent3D _agent;
    private AiStateMachine _aiStateMachine;
    private AnimationTree _animTree;
    private float _gravity;
    private bool _isLookingAtPlayer;
    private bool _isRunning;
    private bool _isStopped;
    private float _targetYRotation;
    [Export] public float MaxBlendSpeed = 3f;
    [Export] public float RunAcceleration = 8f;
    [Export] public float RunSpeed = 3;
    [Export] public float WalkAcceleration = 2f;
    [Export] public float WalkSpeed = 1;
    public float PlayerDistance { get; private set; }
    public CharacterBody3D Player { get; private set; }

    public override void _Ready()
    {
        _agent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        Player = GetTree().Root.GetNode<CharacterBody3D>("Main/Player");
        _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
        _aiStateMachine = GetNode<AiStateMachine>("AiStateMachine");
        _animTree = GetNode<AnimationTree>("AnimationTree");

        // Signal -> state-machine callback
        _agent.TargetReached += OnTargetReached;
    }

    private void OnTargetReached()
    {
        _aiStateMachine.OnNavigationAgent3dTargetReached();
    }

    public override void _Process(double delta)
    {
        if (Player != null)
            PlayerDistance = Position.DistanceTo(Player.Position);
    }

    public override void _PhysicsProcess(double delta)
    {
        // normal gravity + slide
        if (!IsOnFloor())
            Velocity = Velocity with { Y = Velocity.Y - _gravity * (float)delta };

        // steering
        var moveDir = Vector3.Zero;

        if (!_agent.IsNavigationFinished() && !_isStopped)
        {
            var targetPos = _agent.GetNextPathPosition();
            moveDir = Position.DirectionTo(targetPos);
            moveDir.Y = 0;
            moveDir = moveDir.Normalized();
        }

        var targetXz = moveDir * (_isRunning ? RunSpeed : WalkSpeed);
        var accel = _isRunning ? RunAcceleration : WalkAcceleration;

        var currentXz = new Vector3(Velocity.X, 0, Velocity.Z);

        var newXz = currentXz.MoveToward(targetXz, accel * (float)delta);

        Velocity = Velocity with { X = newXz.X, Z = newXz.Z };

        MoveAndSlide();

        UpdateBlend();

        if (_isLookingAtPlayer)
        {
            var dir = Player.Position - Position;
            _targetYRotation = Mathf.Atan2(dir.X, dir.Z);
        }
        else if (Velocity.Length() > 0.01f)
        {
            _targetYRotation = Mathf.Atan2(Velocity.X, Velocity.Z);
        }

        Rotation = Rotation with
        {
            Y = Mathf.LerpAngle(Rotation.Y, _targetYRotation, 0.1f)
        };
    }

    private void UpdateBlend()
    {
        var speed = new Vector3(Velocity.X, 0, Velocity.Z).Length();

        var blendPos = Mathf.Clamp(speed / MaxBlendSpeed * 3f, 0f, 3f);

        _animTree.Set("parameters/blend_position", blendPos);
    }

    public void MoveToPosition(Vector3 target, bool adjust = true)
    {
        _agent ??= GetNode<NavigationAgent3D>("NavigationAgent3D");

        _isStopped = false;

        if (adjust)
        {
            var mapRid = GetWorld3D().NavigationMap;
            var safeTarget = NavigationServer3D.Singleton
                .MapGetClosestPoint(mapRid, target);
            _agent.SetTargetPosition(safeTarget);
        }
        else
        {
            _agent.SetTargetPosition(target);
        }
    }

    public void SetIsRunning(bool isRunning)
    {
        _isRunning = isRunning;
    }

    private void SetIsStopped(bool isStopped)
    {
        _isStopped = isStopped;
    }
}