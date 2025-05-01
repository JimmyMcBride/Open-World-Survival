using OpenWorldSurvival.scripts.signals;

namespace OpenWorldSurvival.scripts;

public partial class SmelterStationInteraction : InteractableObject
{
    public override void OnInteract()
    {
        GlobalSignals.Instance.EmitOnOpenCraftingMenu("smelting");
    }
}