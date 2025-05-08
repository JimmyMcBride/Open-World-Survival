using Godot;
using Item = OpenWorldSurvival.game.scripts.systems.inventory.item.Item;

namespace OpenWorldSurvival.game.globals.signals;

public partial class GlobalSignals : Node
{
    [Signal]
    public delegate void OnCloseCraftingMenuEventHandler(string craftingType);

    [Signal]
    public delegate void OnEnemyInRangeEventHandler(bool isColliding);

    [Signal]
    public delegate void OnInteractableCollisionEventHandler(bool isColliding);

    [Signal]
    public delegate void OnItemPickedUpEventHandler(Item item, int amount);

    [Signal]
    public delegate void OnOpenCraftingMenuEventHandler(string craftingType);

    public static GlobalSignals Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void EmitOnItemPickedUp(Item item, int amount)
    {
        EmitSignal(SignalName.OnItemPickedUp, item, amount);
    }

    public void EmitOnOpenCraftingMenu(string craftingType)
    {
        EmitSignal(SignalName.OnOpenCraftingMenu, craftingType);
    }

    public void EmitOnCloseCraftingMenu(string craftingType)
    {
        EmitSignal(SignalName.OnCloseCraftingMenu, craftingType);
    }

    public void EmitOnInteractableColliding(bool isColliding)
    {
        EmitSignal(SignalName.OnInteractableCollision, isColliding);
    }

    public void EmitOnEnemyInRange(bool isColliding)
    {
        EmitSignal(SignalName.OnEnemyInRange, isColliding);
    }
}