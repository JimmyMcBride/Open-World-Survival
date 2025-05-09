using Godot;
using OpenWorldSurvival.engine.core;
using OpenWorldSurvival.game.scripts.systems.vitals;

namespace OpenWorldSurvival.game.scripts.actors.ai;

public partial class Attack : AiState
{
    [Export] private float _attackRate = 2;
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
        Controller.AnimTree.Set("parameters/conditions/combatIdle", false);
    }

    public override void Enter()
    {
        base.Enter();
        Controller.SetIsStopped(true);
        Controller.SetIsLookingAtPlayer(true);
        _timeSinceLastAttack = _attackRate;
        Controller.AnimTree.Set("parameters/conditions/combatIdle", true);
    }

    private void AttackTarget()
    {
        _timeSinceLastAttack = 0f;
        Controller.AnimTree.Set("parameters/conditions/isAttacking", true);
        GetTree().CreateTimer(0.6f).Timeout += () =>
        {
            Controller.Player.GetNode<Health>("Health").TakeDamage(_damage);
            Controller.AnimTree.Set("parameters/conditions/isAttacking", false);
        };
    }

    private bool CanAttack()
    {
        return _timeSinceLastAttack > _attackRate;
    }
}