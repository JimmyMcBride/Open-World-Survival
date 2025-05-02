using Godot;
using OpenWorldSurvival.game.globals.constants;

namespace OpenWorldSurvival.game.scripts.systems.interactions;

public partial class InteractionController : RayCast3D
{
    private Label _interactionPrompt;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _interactionPrompt = GetNode<Label>("InteractionPrompt");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        HandleInteractionPrompt();
    }

    private void HandleInteractionPrompt()
    {
        var myObject = GetCollider();
        _interactionPrompt.Text = "";

        if (myObject == null || !myObject.HasMethod("OnInteract")) return;

        var interactable = (InteractableObject)myObject;

        if (!interactable.CanInteract) return;
        _interactionPrompt.Text = interactable.InteractPrompt;

        if (Input.IsActionJustPressed(InputAction.Accept)) interactable.OnInteract();
    }
}