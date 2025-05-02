using Godot;
using OpenWorldSurvival.game.scripts.systems.inventory;

namespace OpenWorldSurvival.game.scripts.systems.crafting;

[GlobalClass]
public partial class CraftingRecipeRequirement : Resource
{
    [Export] public int Amount;
    [Export] public Item Item;
}