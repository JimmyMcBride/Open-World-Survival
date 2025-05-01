using Godot;

namespace OpenWorldSurvival.scripts;

public partial class Flee : AiState
{
    [Export] public float FleeDistance = 15;

    public override void Enter()
    {
        base.Enter();
        Controller.SetIsRunning(true);
        FleeFromPlayer();
    }

    public override void Exit()
    {
        base.Exit();
        Controller.SetIsRunning(false);
    }

    private void FleeFromPlayer()
    {
        var delta = Controller.Position - Controller.Player.Position;
        delta.Y = 0; // stay on the nav-mesh plane
        var dir = delta.Normalized();

        var rawTarget = Controller.Position + dir * FleeDistance;
        var mapRid = Controller.GetWorld3D().NavigationMap;
        var safeTarget = NavigationServer3D.Singleton
            .MapGetClosestPoint(mapRid, rawTarget);

        Controller.MoveToPosition(safeTarget);
    }

    public override void NavigationComplete()
    {
        StateMachine.ChangeState("Wander");
    }
}