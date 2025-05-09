using System;
using Godot;

namespace OpenWorldSurvival.game.scripts.actors.ai;

public partial class AiState : Node
{
    protected static readonly Random Rng = new();
    [Export] private NodePath _controllerPath;

    protected AbstractAiController Controller { get; private set; }
    protected AiStateMachine StateMachine { get; private set; }
    protected bool IsActive { get; private set; }

    public void Initialize()
    {
        StateMachine = GetParent<AiStateMachine>();
        Controller = GetNode<AbstractAiController>(_controllerPath);
    }

    public virtual void Enter()
    {
        IsActive = true;
    }

    public virtual void Exit()
    {
        IsActive = false;
    }

    public virtual void Update(double delta)
    {
    }

    public virtual void PhysicsUpdate(double delta)
    {
    }

    public virtual void NavigationComplete()
    {
    }

    protected Vector3 RandomOffset()
    {
        var x = (float)(Rng.NextDouble() * 2.0 - 1.0);
        var z = (float)(Rng.NextDouble() * 2.0 - 1.0);
        return new Vector3(x, 0, z).Normalized();
    }
}