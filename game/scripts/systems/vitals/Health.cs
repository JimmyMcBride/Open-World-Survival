using System;
using Godot;

namespace OpenWorldSurvival.game.scripts.systems.vitals;

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

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        EmitSignalOnHealthChanged(CurrentHealth, MaxHealth);
        EmitSignalOnTakeDamage();

        if (CurrentHealth <= 0) Die();
    }

    public void Die()
    {
        EmitSignalOnDie();

        if (_dropOnDeath != null)
        {
            var drop = _dropOnDeath.Instantiate<Node3D>();
            GetNode("/root/").AddChild(drop);
            drop.Position = GetParent<Node3D>().Position;
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