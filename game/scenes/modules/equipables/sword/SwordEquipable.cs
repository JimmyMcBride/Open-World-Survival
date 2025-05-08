using Godot;
using OpenWorldSurvival.engine.core;
using OpenWorldSurvival.engine.godot;
using OpenWorldSurvival.game.scripts.systems.equip;
using OpenWorldSurvival.game.scripts.systems.vitals;

namespace OpenWorldSurvival.game.scenes.modules.equipables.sword;

public partial class SwordEquipable : EquipObject
{
    [Export] private float _attackRate = 0.7f;
    private float _attackTimer;
    private string _currentAttackState = "idle";
    [Export] private int _damage = 15;
    private bool _followUpQueued;
    private Area3D _hitCollider;
    private float _timeSinceLastAttack;

    public override void _Ready()
    {
        Initialize();
        _hitCollider = GetNode<Area3D>("HitCollider");
        _timeSinceLastAttack = _attackRate;
        _currentAttackState = "idle";
        _followUpQueued = false;
        _attackTimer = 0f;
    }

    public override void SetHand(bool isMainHand)
    {
        base.SetHand(isMainHand);
        if (isMainHand) return;
        var visual = GetNode<Node3D>("Visual");
        visual.Position = visual.Position.SetX(-visual.Position.X);
        visual.Rotation = visual.Rotation.SetY(float.Abs(visual.Rotation.Y));
    } 
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _timeSinceLastAttack += (float)delta;
        _attackTimer += (float)delta;

        if (_currentAttackState == "attack" && _followUpQueued && _attackTimer >= 0.4f)
        {
            AnimationPlayer.Stop();
            AnimationPlayer.Play(IsMainHand ? "attack_2" : "attack_2_off");
            _currentAttackState = "attack_2";
            _timeSinceLastAttack = 0f;
            _attackTimer = 0f;
            _followUpQueued = false;
        }
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

        if (_currentAttackState == "attack" && _attackTimer is >= 0.1f and <= 0.4f)
        {
            _followUpQueued = true;
            return;
        }

        if (!CanAttack()) return;
        AnimationPlayer.Stop();
        AnimationPlayer.Play(IsMainHand ? "attack" : "attack_off");
        _currentAttackState = "attack";
        _timeSinceLastAttack = 0f;
        _attackTimer = 0f;
        _followUpQueued = false;
    }

    private void OnHit()
    {
        var bodies = _hitCollider.GetOverlappingBodies();

        foreach (var body in bodies)
        {
            if (!body.HasNode("Health") || body == Player) continue;
            var health = body.GetNode<Health>("Health");
            var modifier = IsMainHand ? 1 : .5f;
            var damage = (int)(_damage * modifier);
            health.TakeDamage(damage);
        }
    }

    private bool CanAttack()
    {
        return _currentAttackState == "idle" && _timeSinceLastAttack >= _attackRate;
    }
}