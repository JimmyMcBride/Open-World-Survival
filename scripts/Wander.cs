using Godot;

namespace OpenWorldSurvival.scripts;

public partial class Wander : AiState
{
    private float _chaseRange = 10;
    private Vector3 _home;

    [Export] public float MaxWaitTime = 2f;
    [Export] public float MaxWanderRange = 6f;
    [Export] public float MinWaitTime = .2f;

    public override void Enter()
    {
        base.Enter();
        _home = Controller.Position;
        var wait = Mathf.Lerp(MinWaitTime, MaxWaitTime, Rng.NextSingle());
        GetTree().CreateTimer(wait).Timeout += NewDestination;
    }

    private void NewDestination()
    {
        var scalar = Rng.NextSingle() * MaxWanderRange;
        var dest = _home + RandomOffset() * scalar;
        Controller.MoveToPosition(dest);
    }

    public override void NavigationComplete()
    {
        var wait = Mathf.Lerp(MinWaitTime, MaxWaitTime, Rng.NextSingle());
        if (!IsActive) return;
        GetTree().CreateTimer(wait).Timeout += NewDestination;
    }

    public override void Update(double delta)
    {
        if (Controller.PlayerDistance < _chaseRange) StateMachine.ChangeState("Chase");
    }
}