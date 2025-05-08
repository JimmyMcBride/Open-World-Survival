using Godot;
using OpenWorldSurvival.game.scripts.systems.inventory.item;

namespace OpenWorldSurvival.game.scripts.systems.crafting;

[GlobalClass]
public partial class CraftingRecipeRequirement : Resource
{
    [Export] public int Amount;
    [Export] public Item Item;
}