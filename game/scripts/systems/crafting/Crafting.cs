using System.Collections.Generic;
using Godot;
using Godot.Collections;
using GlobalSignals = OpenWorldSurvival.game.globals.signals.GlobalSignals;
using Inventory = OpenWorldSurvival.game.scripts.systems.inventory.Inventory;

namespace OpenWorldSurvival.game.scripts.systems.crafting;

public partial class Crafting : Node
{
    private readonly List<CraftingRecipeUi> _recipeUiList = new();
    private Inventory _inventory;
    private VBoxContainer _uiParent;
    private Panel _window;

    [Export] public PackedScene CraftingRecipeUiScene;
    [Export] public string CraftingTypeName;
    [Export] public Array<CraftingRecipe> Recipes = new();

    public override void _Ready()
    {
        _window = GetNode<Panel>("CraftingWindow");
        _uiParent = GetNode<VBoxContainer>("CraftingWindow/VBoxContainer");
        _inventory = GetParent().GetNode<Inventory>("Inventory");

        foreach (var craftingRecipe in Recipes)
        {
            var recipeNode = CraftingRecipeUiScene.Instantiate<CraftingRecipeUi>();
            _uiParent.AddChild(recipeNode);
            recipeNode.CraftingRecipe = craftingRecipe;
            recipeNode.Crafting = this;
            _recipeUiList.Add(recipeNode);
        }

        GlobalSignals.Instance.OnOpenCraftingMenu += OpenCraftingWindow;
        GlobalSignals.Instance.OnCloseCraftingMenu += CloseCraftingWindow;

        CloseCraftingWindow(CraftingTypeName);
    }

    public void Craft(CraftingRecipe craftingRecipe)
    {
        foreach (var req in craftingRecipe.Requirements)
            _inventory.RemoveItem(req.Item, req.Amount);

        _inventory.AddItem(craftingRecipe.Item);
        UpdateRecipeUi();
    }

    private void OpenCraftingWindow(string craftingType)
    {
        if (craftingType != CraftingTypeName) return;
        _window.Visible = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;

        UpdateRecipeUi();
    }

    private void CloseCraftingWindow(string craftingType)
    {
        if (craftingType != CraftingTypeName) return;
        _window.Visible = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private void UpdateRecipeUi()
    {
        foreach (var recipeUi in _recipeUiList) recipeUi.UpdateRecipe(_inventory);
    }

    public bool IsCraftingWindowOpen()
    {
        return _window.Visible;
    }
}