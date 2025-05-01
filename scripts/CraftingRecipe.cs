using Godot;
using Godot.Collections;

namespace OpenWorldSurvival.scripts;

[GlobalClass]
public partial class CraftingRecipe : Resource
{
    [Export] public Item Item;
    [Export] public Array<CraftingRecipeRequirement> Requirements = new();
}