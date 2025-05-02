using Godot;
using OpenWorldSurvival.engine.core;
using OpenWorldSurvival.game.scripts.systems.inventory;

namespace OpenWorldSurvival.game.scripts.systems.crafting;

public partial class CraftingRecipeUi : Panel
{
    private Button _craftButton;
    private TextureRect _icon;
    private Label _recipeText;
    public Crafting Crafting;
    public CraftingRecipe CraftingRecipe;

    public override void _Ready()
    {
        GetNodes();
        _craftButton.Pressed += OnCraftButtonPressed;
    }

    private void GetNodes()
    {
        _icon = GetNode<TextureRect>("ItemIcon");
        _recipeText = GetNode<Label>("RecipeText");
        _craftButton = GetNode<Button>("CraftButton");
    }

    private void OnCraftButtonPressed()
    {
        Crafting.Craft(CraftingRecipe);
    }

    public void UpdateRecipe(Inventory inventory)
    {
        var canCraft = true;


        foreach (var requirement in CraftingRecipe.Requirements)
            if (inventory.GetNumberOfItems(requirement.Item) < requirement.Amount)
            {
                Logger.Debug("Required items in inventory: " + inventory.GetNumberOfItems(requirement.Item));
                Logger.Debug("Required amount for recipe: " + requirement.Amount);
                canCraft = false;
            }

        Logger.Debug("Can craft: " + canCraft);
        _craftButton.Visible = canCraft;

        _recipeText.Text = CraftingRecipe.Item.Name + "\n";

        foreach (var requirement in CraftingRecipe.Requirements)
        {
            var ownedAmount = inventory.GetNumberOfItems(requirement.Item);
            _recipeText.Text += $"{ownedAmount}/{requirement.Amount} {requirement.Item.Name}\n";
        }

        _icon.Texture = CraftingRecipe.Item.Icon;
    }
}