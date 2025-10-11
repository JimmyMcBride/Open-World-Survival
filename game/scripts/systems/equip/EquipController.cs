using Godot;
using OpenWorldSurvival.engine.core;
using OpenWorldSurvival.game.scripts.actors.player;
using OpenWorldSurvival.game.scripts.systems.inventory.item;

namespace OpenWorldSurvival.game.scripts.systems.equip;

[GlobalClass]
public partial class EquipController : Node
{
    private EquipItem _mainHandItem;
    private EquipObject _mainHandObject;
    private Node3D _mainHandOrigin;
    private bool _offHandDisabled; // True when a 2H weapon is equipped
    private EquipItem _offHandItem;
    private EquipObject _offHandObject;
    private Node3D _offHandOrigin;

    public override void _Ready()
    {
        var parent = GetParent<FirstPersonController>();
        _mainHandOrigin = parent.GetNode<Node3D>("Head/Camera/MainHandOrigin");
        _offHandOrigin = parent.GetNode<Node3D>("Head/Camera/OffHandOrigin");
        _offHandDisabled = false;
    }

    public void EquipMainHand(EquipItem item)
    {
        if (_mainHandItem == item)
        {
            UnequipMainHand();
            return;
        }

        // Unequip existing main hand item
        UnequipMainHand();

        // If equipping a 2H weapon, unequip off-hand and disable it
        if (item.IsTwoHanded)
        {
            UnequipOffHand();
            _offHandDisabled = true;
        }

        // Instantiate and equip the new main hand item
        var obj = item.EquipScene.Instantiate<EquipObject>();
        _mainHandOrigin.AddChild(obj);
        _mainHandObject = obj;
        _mainHandItem = item;

        // var parent = GetParent<FirstPersonController>();
        // Log.Debug($"Setting main hand equip object player to {parent.Name}");
        // _mainHandObject.SetPlayer(parent);
        Log.Debug($"Equipped {item.Name} to main hand");
        _mainHandObject.SetHand(true); // True for the main hand
    }

    public void EquipOffHand(EquipItem item)
    {
        if (_offHandDisabled)
        {
            Log.Debug("Cannot equip off-hand: 2H weapon is equipped.");
            return;
        }

        if (_offHandItem == item)
        {
            UnequipOffHand();
            return;
        }

        // Check if the item can be equipped in off-hand
        if (item.Type == EquipItem.EquipmentType.Weapon && item.IsTwoHanded)
        {
            Log.Debug("Cannot equip 2H weapon in off-hand.");
            return;
        }

        // Unequip existing off-hand item
        UnequipOffHand();

        // Instantiate and equip the new off-hand item
        var obj = item.EquipScene.Instantiate<EquipObject>();
        _offHandOrigin.AddChild(obj);
        _offHandObject = obj;
        _offHandItem = item;

        // var parent = GetParent<FirstPersonController>();
        // Log.Debug($"Setting off-hand equip object player to {parent.Name}");
        // _offHandObject.SetPlayer(parent);
        Log.Debug($"Equipped {item.Name} to off hand");
        _offHandObject.SetHand(false); // False for off-hand
    }

    public void UnequipMainHand()
    {
        if (_mainHandItem == null) return;

        _mainHandObject.QueueFree();
        _mainHandObject = null;
        _mainHandItem = null;

        // Re-enable off-hand if a 2H weapon was unequipped
        _offHandDisabled = false;
    }

    public void UnequipOffHand()
    {
        if (_offHandItem == null) return;
        Log.Debug("Unequping off hand");

        _offHandObject.QueueFree();
        _offHandObject = null;
        _offHandItem = null;
    }

    public bool IsOffHandDisabled()
    {
        return _offHandDisabled;
    }

    public EquipItem GetMainHandItem()
    {
        return _mainHandItem;
    }

    public EquipItem GetOffHandItem()
    {
        return _offHandItem;
    }
}