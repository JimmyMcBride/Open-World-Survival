using Godot;
using OpenWorldSurvival.game.scripts.systems.equip;

namespace OpenWorldSurvival.game.scripts.systems.inventory.item;

[GlobalClass]
public partial class EquipItem : Item
{
    [Export] public PackedScene EquipScene;

    public override bool OnUse(CharacterBody3D character)
    {
        character.GetNode<EquipController>("EquipController").Equip(this);
        return true;
    }
}