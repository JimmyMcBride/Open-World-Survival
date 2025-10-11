using Godot;
using OpenWorldSurvival.engine.core;
using OpenWorldSurvival.game.scripts.systems.vitals;

namespace OpenWorldSurvival.game.scripts.actors.common;

[GlobalClass]
public partial class FallDamage : Node
{
    private CharacterBody3D _character;
    private Health _health;
    private bool _onFloorLastFrame = true;
    private float _yVelocityLastFrame;
    [Export] public float DeathYThreshold = -50;
    [Export] public float FallDamageMultiplier = 1.65f;
    [Export] public float FallDamageThreshold = -13.5f;

    public override void _Ready()
    {
        _character = GetParent<CharacterBody3D>();
        _health = GetParent().GetNode<Health>("Health");
    }

    public override void _Process(double delta)
    {
        if (_character.GlobalPosition.Y < DeathYThreshold)
        {
            _health.Die();
            return; // Exit early to avoid further processing
        }

        if (_character.IsOnFloor() && !_onFloorLastFrame && _yVelocityLastFrame < FallDamageThreshold)
            CalculateDamage();

        _onFloorLastFrame = _character.IsOnFloor();
        _yVelocityLastFrame = _character.Velocity.Y;
    }

    private void CalculateDamage()
    {
        var velocityExcess = Mathf.Abs(_yVelocityLastFrame) - Mathf.Abs(FallDamageThreshold);
        var damagePercentage = velocityExcess * FallDamageMultiplier / 10f;
        var damage = (int)(_health.MaxHealth * damagePercentage);
        Log.Debug($"Velocity excess: {velocityExcess}, damage: {damage}, damage percentage: {damagePercentage}");

        _health.TakeDamage(damage);
    }
}