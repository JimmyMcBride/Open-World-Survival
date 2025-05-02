using Godot;
using OpenWorldSurvival.engine.core;
using OpenWorldSurvival.game.globals.signals;
using OpenWorldSurvival.game.scripts.systems.inventory;
using InteractableObject = OpenWorldSurvival.game.scripts.systems.interactions.InteractableObject;

namespace OpenWorldSurvival.game.scenes.modules.interactables;

public partial class ItemInteraction : InteractableObject
{
    private Inventory _inventory;
    private Item _itemToGive;

    [Export] public string ItemName;
    [Export] public int QuantityToGive = 1;

    public override void _Ready()
    {
        base._Ready();
        _inventory = GetTree().Root.GetNode<Inventory>("Main/Player/Inventory");
        _itemToGive = GD.Load<Item>("res://game/scripts/data/items/" + ItemName + ".tres");
    }


    public override void OnInteract()
    {
        Logger.Info("Item " + ItemName + " was picked up!");
        if (_inventory != null && _itemToGive != null)
        {
            GlobalSignals.Instance.EmitOnItemPickedUp(_itemToGive, QuantityToGive);
            QueueFree();
        }
        else
        {
            Logger.Error("Missing item or inventory reference!");
        }
    }
}