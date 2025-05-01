using Godot;

namespace OpenWorldSurvival.scripts.signals;

public partial class GlobalSignals : Node
{
    [Signal]
    public delegate void OnCloseCraftingMenuEventHandler(string craftingType);

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
}