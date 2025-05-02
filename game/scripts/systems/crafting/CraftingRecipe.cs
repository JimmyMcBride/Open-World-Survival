using Godot;
using Godot.Collections;
using OpenWorldSurvival.game.scripts.systems.inventory;

namespace OpenWorldSurvival.game.scripts.systems.crafting;

[GlobalClass]
public partial class CraftingRecipe : Resource
{
    [Export] public Item Item;
    [Export] public Array<CraftingRecipeRequirement> Requirements = new();
}