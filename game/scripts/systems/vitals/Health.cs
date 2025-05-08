using System;
using Godot;

namespace OpenWorldSurvival.game.scripts.systems.vitals;

[GlobalClass]
public partial class Health : Node
{
    [Signal]
    public delegate void OnDieEventHandler();

    [Signal]
    public delegate void OnHealthChangedEventHandler(int current, int max);

    [Signal]
    public delegate void OnTakeDamageEventHandler();

    [Export] private PackedScene _dropOnDeath;
    [Export] private PostDeathAction _postDeathAction;

    public int CurrentHealth { get; private set; }
    [Export] public int MaxHealth { get; private set; }

    private bool _isDead; // Flag to prevent multiple Die() calls

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
        _isDead = false; // Initialize flag
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return; // Exit if already dead

        CurrentHealth -= damage;
        EmitSignalOnHealthChanged(CurrentHealth, MaxHealth);
        EmitSignalOnTakeDamage();

        if (CurrentHealth <= 0) Die();
    }

    public void Die()
    {
        if (_isDead) return; // Prevent re-execution
        _isDead = true; // Mark as dead

        EmitSignalOnDie();

        if (_dropOnDeath != null)
        {
            var drop = _dropOnDeath.Instantiate<Node3D>();
            GetNode("/root/").AddChild(drop);
            drop.Position = drop.Position with
            {
                Y = GetParent<Node3D>().Position.Y + 1.5f,
                Z = GetParent<Node3D>().Position.Z,
                X = GetParent<Node3D>().Position.X
            };
        }

        switch (_postDeathAction)
        {
            case PostDeathAction.DestroyNode:
                GetParent<Node3D>().QueueFree();
                break;
            case PostDeathAction.RestartScene:
                GetTree().ReloadCurrentScene();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Heal(int amount)
    {
        if (_isDead) return; // Prevent healing if dead

        CurrentHealth += amount;

        if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;

        EmitSignalOnHealthChanged(CurrentHealth, MaxHealth);
    }

    private enum PostDeathAction
    {
        DestroyNode,
        RestartScene
    }
}