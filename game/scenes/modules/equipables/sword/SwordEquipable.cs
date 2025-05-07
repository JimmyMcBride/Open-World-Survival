using Godot;
using OpenWorldSurvival.game.scripts.systems.equip;
using OpenWorldSurvival.game.scripts.systems.vitals;

namespace OpenWorldSurvival.game.scenes.modules.equipables.sword;

public partial class SwordEquipable : EquipObject
{
    [Export] private float _attackRate = 0.7f;
    [Export] private int _damage = 15;
    private Area3D _hitCollider;
    private float _lastAttackTime;
    private float _timeSinceLastAttack;
    private string _currentAttackState = "idle"; // Tracks "idle", "attack", or "attack_2"
    private bool _followUpQueued;
    private float _attackTimer; // Tracks progress of current attack animation

    public override void _Ready()
    {
        Initialize();
        _hitCollider = GetNode<Area3D>("HitCollider");
        _timeSinceLastAttack = _attackRate;
        _currentAttackState = "idle";
        _followUpQueued = false;
        _attackTimer = 0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _timeSinceLastAttack += (float)delta;
        _attackTimer += (float)delta;

        // Handle transition from attack to attack_2 if follow-up is queued
        if (_currentAttackState == "attack" && _followUpQueued && _attackTimer >= 0.4f)
        {
            AnimationPlayer.Stop();
            AnimationPlayer.Play("attack_2");
            _currentAttackState = "attack_2";
            _timeSinceLastAttack = 0f;
            _attackTimer = 0f;
            _followUpQueued = false;
        }
        // Reset to idle after attack or attack_2 completes
        else if (_currentAttackState != "idle" && _attackTimer >= _attackRate)
        {
            _currentAttackState = "idle";
            _followUpQueued = false;
            _attackTimer = 0f;
        }
    }

    protected override void OnPrimaryAction()
    {
        if (!Player.IsInAttackableState()) return;

        // Check if we can queue a follow-up attack during attack (0.3–0.4 seconds)
        if (_currentAttackState == "attack" && _attackTimer is >= 0.1f and <= 0.4f)
        {
            _followUpQueued = true;
            return;
        }

        // Start a new attack if allowed
        if (!CanAttack()) return;
        AnimationPlayer.Stop();
        AnimationPlayer.Play("attack");
        _currentAttackState = "attack";
        _timeSinceLastAttack = 0f;
        _attackTimer = 0f;
        _followUpQueued = false;
    }

    protected override void OnSecondaryAction()
    {
        // No secondary action implemented
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
        return _currentAttackState == "idle" && _timeSinceLastAttack >= _attackRate;
    }
}