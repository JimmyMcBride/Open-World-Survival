using Godot;
using OpenWorldSurvival.scripts.signals;
using OpenWorldSurvival.scripts.utils;

namespace OpenWorldSurvival.scripts;

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
        _itemToGive = GD.Load<Item>("res://items/" + ItemName + ".tres");
    }


    public override void OnInteract()
    {
        Logger.Info("Item " + ItemName + " was used!");
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