using Godot;
using OpenWorldSurvival.game.scripts.systems.equip;
using OpenWorldSurvival.game.scripts.systems.vitals;

namespace OpenWorldSurvival.game.scenes.modules.equipables.sword;

public partial class SwordEquipable : EquipObject
{
    [Export] private float _attackRate = .7f;
    [Export] private int _damage = 15;
    private Area3D _hitCollider;
    private float _lastAttackTime;
    private float _timeSinceLastAttack;

    public override void _Ready()
    {
        Initialize();
        _hitCollider = GetNode<Area3D>("HitCollider");
        _timeSinceLastAttack = _attackRate;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _timeSinceLastAttack += (float)delta;
    }

    protected override void OnPrimaryAction()
    {
        if (!CanAttack()) return;
        AnimationPlayer.Stop();
        AnimationPlayer.Play("attack");
        _timeSinceLastAttack = 0f;
    }

    protected override void OnSecondaryAction()
    {
    }

    private void OnHit()
    {
        var bodies = _hitCollider.GetOverlappingBodies();

        foreach (var body in bodies)
        {
            if (!body.HasNode("Health") || body == Player) continue;
            var health = body.GetNode<Health>("Health");
            health.TakeDamage(_damage);
        }
    }

    private bool CanAttack()
    {
        return _timeSinceLastAttack > _attackRate && Player.IsInAttackableState();
    }
}