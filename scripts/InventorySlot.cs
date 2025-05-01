using Godot;
using OpenWorldSurvival.scripts.utils;

namespace OpenWorldSurvival.scripts;

public partial class InventorySlot : Button
{
    private TextureRect _icon;
    private Label _quantityText;
    public Inventory Inventory;
    public Maybe<Item> Item;
    public int Quantity;

    public override void _Ready()
    {
        _icon = GetNode<TextureRect>("Icon");
        _quantityText = GetNode<Label>("QuantityText");

        MouseEntered += OnMouseEntered;
    }

    private void OnMouseEntered()
    {
        // Handle what happens when the mouse enters the button area
    }

    public override void _Process(double delta)
    {
    }

    public void SetItem(Item item, int quantity)
    {
        Item = Maybe<Item>.Some(item);
        Quantity = quantity;
        _icon.Texture = item.Icon;
        UpdateQuantityText();
    }

    public void ClearItem()
    {
        Item = Maybe<Item>.None();
        Quantity = 0;
        _icon.Texture = null;
        UpdateQuantityText();
    }

    public void AddItem(Item item, int quantity)
    {
        if (Item.IsNone)
        {
            SetItem(item, quantity);
            return;
        }

        if (Item.UnwrapOrDefault() != item) return;

        Quantity += quantity;
        UpdateQuantityText();
    }

    public void RemoveItem(int quantity)
    {
        Logger.Debug("Removing Item");
        Quantity -= quantity;
        UpdateQuantityText();

        if (Quantity > 0) return;
        ClearItem();
    }

    private void UpdateQuantityText()
    {
        _quantityText.Text = Quantity == 0 ? "" : Quantity.ToString();
    }

    private void DropItem()
    {
        Logger.Debug("Dropping Item");

        Item.Match(
            some =>
            {
                var worldItem = (ItemInteraction)some.WorldItemScene?.Instantiate();
                if (worldItem == null)
                {
                    Logger.Error("Item is null! :(");
                    return;
                }

                Logger.Debug("Item is not null");

                // How far in front of the feet you want the item to appear
                const float dropDistance = 1.5f;

                // Character that owns the inventory
                var body = (FirstPersonController.FirstPersonController)Inventory.GetParent<CharacterBody3D>();

                // Character’s forward direction in world space (flattened to ground plane)
                var forward = -body.GetHeadTransform().Basis.Z; // Godot’s +Z is “backwards”
                forward.Y = 0f;
                forward = forward.Normalized();

                // Final spawn point
                var dropPos = body.GetHeadTransform().Origin // player position
                              + Vector3.Up * .1f // chest–height
                              + forward * dropDistance; // in front

                worldItem.Position = dropPos;

                GetTree().CurrentScene.AddChild(worldItem);

                RemoveItem(1);
                // Notify Inventory to reorganize if the slot is empty
                if (Quantity == 0)
                    Inventory.ReorganizeInventory();
            },
            () => { Logger.Error("Item is null!"); }
        );
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent || !mouseEvent.IsPressed()) return;
        if (mouseEvent.ButtonIndex == MouseButton.Right)
            DropItem();
    }
}