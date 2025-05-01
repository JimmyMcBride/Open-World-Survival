using Godot;

namespace OpenWorldSurvival.scripts;

[GlobalClass]
public partial class CraftingRecipeRequirement : Resource
{
    [Export] public int Amount;
    [Export] public Item Item;
}