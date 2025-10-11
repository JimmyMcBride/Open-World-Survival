using Godot;
using OpenWorldSurvival.engine.core;
using OpenWorldSurvival.game.scripts.systems.equip;

namespace OpenWorldSurvival.game.scripts.systems.inventory.item;

[GlobalClass]
public partial class EquipItem : Item
{
    public enum EquipmentType
    {
        Weapon, // Can be equipped in the main hand or off-hand (if 1H)
        Shield // Can only be equipped in off-hand
    }

    [Export] public PackedScene EquipScene;
    [Export] public bool IsTwoHanded;
    [Export] public EquipmentType Type;

    public override bool OnUse(CharacterBody3D character)
    {
        var equipController = character.GetNode<EquipController>("EquipController");
        var mainHandOrigin = character.GetNode<Node3D>("Head/Camera/MainHandOrigin");

        // Shields must go in off-hand
        if (Type == EquipmentType.Shield)
        {
            equipController.EquipOffHand(this);
            return true;
        }

        // Two-handed weapons must go in the main hand
        if (Type == EquipmentType.Weapon && IsTwoHanded)
        {
            equipController.EquipMainHand(this);
            return true;
        }

        // One-handed weapons: equip to the main hand if empty, otherwise off-hand
        Log.Debug($"Main Hand Child Count: {mainHandOrigin.GetChildCount()}");
        if (mainHandOrigin.GetChildCount() == 0)
            equipController.EquipMainHand(this);
        else
            equipController.EquipOffHand(this);

        return true;
    }
}