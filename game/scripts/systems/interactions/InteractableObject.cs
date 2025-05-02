using Godot;

namespace OpenWorldSurvival.game.scripts.systems.interactions;

public abstract partial class InteractableObject : Node3D
{
    [Export] public bool CanInteract = true;
    [Export] public string InteractPrompt;

    public abstract void OnInteract();
}