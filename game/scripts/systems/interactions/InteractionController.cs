using Godot;
using OpenWorldSurvival.game.globals.constants;
using OpenWorldSurvival.game.globals.signals;

namespace OpenWorldSurvival.game.scripts.systems.interactions;

public partial class InteractionController : RayCast3D
{
    private Label _interactionPrompt;
    private bool _isInteracting;
    private bool _previousInteractionState;

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

        if (myObject == null || !myObject.HasMethod("OnInteract"))
        {
            // GlobalSignals.Instance.EmitOnInteractableColliding(false);
            _isInteracting = false;
            if (_isInteracting != _previousInteractionState)
            {
                _previousInteractionState = _isInteracting;
                GlobalSignals.Instance.EmitOnInteractableColliding(_isInteracting);
            }

            return;
        }

        var interactable = (InteractableObject)myObject;

        // GlobalSignals.Instance.EmitOnInteractableColliding(interactable.CanInteract)
        _isInteracting = interactable.CanInteract;
        if (_isInteracting != _previousInteractionState)
        {
            _previousInteractionState = _isInteracting;
            GlobalSignals.Instance.EmitOnInteractableColliding(_isInteracting);
        }

        if (!interactable.CanInteract) return;
        _interactionPrompt.Text = interactable.InteractPrompt;

        if (Input.IsActionJustPressed(InputAction.Accept)) interactable.OnInteract();
    }
}