using OpenWorldSurvival.game.globals.signals;
using InteractableObject = OpenWorldSurvival.game.scripts.systems.interactions.InteractableObject;

namespace OpenWorldSurvival.game.scenes.modules.interactables.crafting;

public partial class CraftingTableInteraction : InteractableObject
{
    public override void OnInteract()
    {
        GlobalSignals.Instance.EmitOnOpenCraftingMenu("crafting");
    }
}