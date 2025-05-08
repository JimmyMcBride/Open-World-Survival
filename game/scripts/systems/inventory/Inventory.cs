using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using OpenWorldSurvival.game.globals.constants;
using OpenWorldSurvival.game.scripts.systems.inventory.item;
using GlobalSignals = OpenWorldSurvival.game.globals.signals.GlobalSignals;
using Logger = OpenWorldSurvival.engine.core.Logger;

namespace OpenWorldSurvival.game.scripts.systems.inventory;

// Class definition for the Inventory system in an Open World Survival game.
public partial class Inventory : Node
{
    private Label _infoText; // UI label to display information about inventory items.
    private Input.MouseModeEnum _lastMouseMode;
    private Array<InventorySlot> _slots = new(); // Array to hold inventory slots.

    [Export]
    private Array<Item> _starterItems = new(); // Exported array for pre-configuring starter items in the Godot editor.

    private Panel _window; // UI panel that acts as the inventory window.

    public override void _Ready()
    {
        _window = GetNode<Panel>("InventoryWindow");
        _window.Visible = false;

        _infoText = GetNode<Label>("InventoryWindow/InfoText");

        var nodes = GetNode("InventoryWindow/SlotContainer").GetChildren();
        foreach (var node in nodes)
            if (node is InventorySlot slot)
            {
                _slots.Add(slot);
                slot.ClearItem();
                slot.Inventory = this;
            }

        foreach (var item in _starterItems)
            AddItem(item);

        _lastMouseMode = Input.MouseMode;

        GlobalSignals.Instance.OnItemPickedUp += AddItem;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(InputAction.Inventory))
            ToggleWindow();

        if (Input.MouseMode == Input.MouseModeEnum.Captured && _lastMouseMode != Input.MouseModeEnum.Captured)
            if (_window.Visible)
                ToggleWindow();

        _lastMouseMode = Input.MouseMode;
    }

    private void ToggleWindow()
    {
        _window.Visible = !_window.Visible;
        Input.MouseMode = _window.Visible ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
    }

    public bool IsOpen()
    {
        return _window.Visible;
    }

    public void AddItem(Item item, int quantity = 1)
    {
        var slot = GetSlotToAdd(item);
        slot.AddItem(item, quantity);
    }

    public void RemoveItem(Item item, int quantity = 1)
    {
        var slot = GetSlotToRemove(item);
        if (slot == null || slot.Item.UnwrapOrDefault() != item)
        {
            Logger.Info("No slot found with item to remove: " + item.Name);
            return;
        }

        Logger.Info("Removing item: " + item.Name + ", quantity: " + quantity);
        slot.RemoveItem(quantity);

        // Reorganize inventory if the slot is now empty
        if (slot.Quantity == 0)
            ReorganizeInventory();
    }

    private InventorySlot GetSlotToAdd(Item item)
    {
        return _slots.FirstOrDefault(slot =>
        {
            var slotItem = slot.Item.UnwrapOrDefault();
            return slotItem == default || (slotItem == item && slot.Quantity < item.MaxStackSize);
        });
    }

    private InventorySlot GetSlotToRemove(Item item)
    {
        return _slots.FirstOrDefault(slot => slot.Item.UnwrapOrDefault() == item);
    }

    public int GetNumberOfItems(Item item)
    {
        return _slots.Where(slot => slot.Item.UnwrapOrDefault() == item).Sum(slot => slot.Quantity);
    }

    public void ReorganizeInventory()
    {
        // Create a list to store non-empty slot data
        var items = new List<(Item Item, int Quantity)>();
        foreach (var slot in _slots)
        {
            var slotItem = slot.Item.UnwrapOrDefault();
            if (slotItem != null && slot.Quantity > 0)
                items.Add((slotItem, slot.Quantity));
            slot.ClearItem(); // Clear all slots
        }

        // Repopulate slots with items in order
        var slotIndex = 0;
        foreach (var (item, quantity) in items)
        {
            if (slotIndex >= _slots.Count)
            {
                Logger.Error("Not enough slots to reorganize inventory!");
                break;
            }

            _slots[slotIndex].SetItem(item, quantity);
            slotIndex++;
        }

        Logger.Info("Inventory reorganized. Occupied slots: " + slotIndex);
    }

    public void SetInfoText(string text)
    {
        _infoText.Text = text;
    }
}