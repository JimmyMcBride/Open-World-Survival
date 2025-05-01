using Godot;

namespace OpenWorldSurvival.scripts;

public partial class PedestalInteraction : InteractableObject
{
    private GpuParticles3D _fire;

    public override void _Ready()
    {
        _fire = GetNode<GpuParticles3D>("Fire");
    }

    public override void OnInteract()
    {
        _fire.Emitting = !_fire.Emitting;
    }
}