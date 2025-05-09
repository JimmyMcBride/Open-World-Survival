using Godot;
using OpenWorldSurvival.engine.core;

namespace OpenWorldSurvival.game.scripts.actors.ai;

public partial class Chase : AiState
{
    private float _pathUpdateRate = .1f;
    private float _timeSincePath;
    [Export] public float LoseInterestRange = 10;
    [Export] public float StopRange = 1.1f;

    public override void Enter()
    {
        base.Enter();
        Controller.SetIsRunning(true);
        Controller.AnimTree.Set("parameters/conditions/chase_or_wander", true);
    }

    public override void Exit()
    {
        base.Exit();
        Controller.SetIsRunning(false);
        Controller.AnimTree.Set("parameters/conditions/chase_or_wander", false);
    }

    public override void Update(double delta)
    {
        base.Update(delta);
        _timeSincePath += (float)delta;
        if (_timeSincePath < _pathUpdateRate) return;
        _timeSincePath = 0;

        if (LoseInterestRange > Controller.PlayerDistance)
            Controller.MoveToPosition(Controller.Player.Position, false);
        else
            StateMachine.ChangeState("Wander");

        if (StopRange >= Controller.PlayerDistance)
            StateMachine.ChangeState("Attack");
    }
}