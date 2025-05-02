using OpenWorldSurvival.game.globals.signals;
using InteractableObject = OpenWorldSurvival.game.scripts.systems.interactions.InteractableObject;

namespace OpenWorldSurvival.game.scenes.modules.interactables.smelting;

public partial class SmelterStationInteraction : InteractableObject
{
    public override void OnInteract()
    {
        GlobalSignals.Instance.EmitOnOpenCraftingMenu("smelting");
    }
}