using System;
using Godot;
using OpenWorldSurvival.engine.core;
using OpenWorldSurvival.game.scripts.actors.player;

namespace OpenWorldSurvival.game.scripts.systems.equip;

public partial class EquipObject : Node3D
{
    protected AnimationPlayer AnimationPlayer;
    protected FirstPersonController Player;
    protected bool IsMainHand { get; private set; }

    protected void Initialize()
    {
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Player = (FirstPersonController)GetParent().GetParent().GetParent().GetParent();
    }

    public virtual void SetHand(bool isMainHand)
    {
        Logger.Debug($"SetHand as main: {isMainHand}");
        IsMainHand = isMainHand;
    }

    protected virtual void OnPrimaryAction()
    {
    }

    protected virtual void OnSecondaryAction()
    {
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouseButton { Pressed: true } action) return;
        switch (action.ButtonIndex)
        {
            case MouseButton.Left:
                OnPrimaryAction();
                break;
            case MouseButton.Right:
                OnSecondaryAction();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(@event));
        }
    }
}