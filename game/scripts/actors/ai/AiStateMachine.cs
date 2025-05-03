using System.Collections.Generic;
using Godot;
using OpenWorldSurvival.engine.core;

namespace OpenWorldSurvival.game.scripts.actors.ai;

public partial class AiStateMachine : Node
{
    private readonly Dictionary<string, AiState> _states = new();
    private AiState _currentState;

    [Export] public AiState DefaultState;

    public override void _Ready()
    {
        foreach (var child in GetChildren())
            if (child is AiState s)
            {
                _states[s.Name] = s;
                s.Initialize();
            }

        if (DefaultState != null)
            ChangeState(DefaultState.Name);
    }

    public void ChangeState(string name)
    {
        if (!_states.TryGetValue(name, out var next) || next == _currentState)
            return;

        _currentState?.Exit();
        _currentState = next;
        next.Enter();

        Logger.Info($"[AI] State changed to {name}");
    }

    public override void _Process(double d)
    {
        _currentState?.Update(d);
    }

    public override void _PhysicsProcess(double d)
    {
        _currentState?.PhysicsUpdate(d);
    }

    public void OnNavigationAgent3dTargetReached()
    {
        _currentState?.NavigationComplete();
    }
}