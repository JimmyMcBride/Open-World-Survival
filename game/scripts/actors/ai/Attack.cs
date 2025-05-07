using Godot;
using OpenWorldSurvival.game.scripts.systems.vitals;

namespace OpenWorldSurvival.game.scripts.actors.ai;

public partial class Attack : AiState
{
    [Export] private float _attackRate = 1;
    [Export] private int _damage = 5;
    private float _timeSinceLastAttack;

    public override void Update(double delta)
    {
        base.Update(delta);
        if (CanAttack()) AttackTarget();
        if (Controller.PlayerDistance > 1.2) StateMachine.ChangeState("Chase");
    }

    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);
        _timeSinceLastAttack += (float)delta;
    }

    public override void NavigationComplete()
    {
        base.NavigationComplete();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Enter()
    {
        base.Enter();
        Controller.SetIsStopped(true);
        Controller.SetIsLookingAtPlayer(true);
        _timeSinceLastAttack = _attackRate;
    }

    private void AttackTarget()
    {
        _timeSinceLastAttack = 0f;
        Controller.Player.GetNode<Health>("Health").TakeDamage(_damage);
    }

    private bool CanAttack()
    {
        return _timeSinceLastAttack > _attackRate;
    }
}