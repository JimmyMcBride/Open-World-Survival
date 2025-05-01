using Godot;

namespace OpenWorldSurvival.scripts;

public abstract partial class InteractableObject : Node3D
{
    [Export] public bool CanInteract = true;
    [Export] public string InteractPrompt;

    public abstract void OnInteract();
}