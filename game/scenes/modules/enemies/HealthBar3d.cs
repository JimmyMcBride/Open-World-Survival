using Godot;
using OpenWorldSurvival.game.scripts.systems.vitals;

namespace OpenWorldSurvival.game.scenes.modules.enemies;

public partial class HealthBar3d : Node3D
{
    private Health _health;
    private ProgressBar _progressBar;

    [Export] public NodePath HealthNodePath { get; set; } // Path to the Health node
    [Export] public Vector3 Offset { get; set; } = new(0, 2, 0); // Offset above the enemy

    public override void _Ready()
    {
        // Get the SubViewport's ProgressBar
        var viewport = GetNode<SubViewport>("SubViewport");
        _progressBar = viewport.GetNode<ProgressBar>("ProgressBar");

        // Get the Health node
        if (!string.IsNullOrEmpty(HealthNodePath)) _health = GetNode<Health>(HealthNodePath);

        // Connect to Health signals
        if (_health == null) return;
        _health.OnHealthChanged += UpdateHealthBar;
        _health.OnDie += OnDie;
        // Initialize the health bar
        UpdateHealthBar(_health.CurrentHealth, _health.MaxHealth);
    }

    private void UpdateHealthBar(int current, int max)
    {
        _progressBar.MaxValue = max;
        _progressBar.Value = current;
    }

    private void OnDie()
    {
        // Hide or destroy the health bar when the enemy dies
        QueueFree();
    }

    public override void _Process(double delta)
    {
        // Position the health bar above the enemy
        // if (_health != null && _health.GetParent<Node3D>() != null)
        //     GlobalPosition = _health.GetParent<Node3D>().GlobalPosition + Offset;
    }

    public void SetHealthNode(Health health)
    {
        _health = health;
        if (_health == null) return;
        _health.OnHealthChanged += UpdateHealthBar;
        _health.OnDie += OnDie;
        UpdateHealthBar(_health.CurrentHealth, _health.MaxHealth);
    }
}