using Godot;
using OpenWorldSurvival.game.scripts.systems.vitals;

namespace OpenWorldSurvival.game.scripts.actors.player;

public partial class HealthBar : ProgressBar
{
    private Health _health;
    private Label _healthLabel;

    public override void _Ready()
    {
        _healthLabel = GetNode<Label>("HealthText");
        _health = GetParent().GetNode<Health>("Health");

        OnHealthChange(_health.CurrentHealth, _health.MaxHealth);
        _health.OnHealthChanged += OnHealthChange;
    }

    private void OnHealthChange(int currentHealth, int maxHealth)
    {
        MaxValue = maxHealth > 0 ? maxHealth : 1;
        Value = currentHealth;
        _healthLabel.Text = $"{currentHealth}/{maxHealth}";
    }
}