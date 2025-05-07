using Godot;
using OpenWorldSurvival.engine.core;
using OpenWorldSurvival.game.scripts.actors.player;
using OpenWorldSurvival.game.scripts.systems.inventory.item;

namespace OpenWorldSurvival.game.scripts.systems.equip;

[GlobalClass]
public partial class EquipController : Node
{
    private EquipItem _currentEquipItem;
    private EquipObject _equipObject;
    private Node3D _equipOrigin;

    public override void _Ready()
    {
        _equipOrigin = GetParent().GetNode<Node3D>("Head/Camera/EquipOrigin");
    }

    public void Equip(EquipItem item)
    {
        if (_currentEquipItem == item)
        {
            Unequip();
            return;
        }

        Unequip();
        var obj = item.EquipScene.Instantiate<EquipObject>();
        _equipOrigin.AddChild(obj);
        _equipObject = obj;
        _currentEquipItem = item;
        var parent = GetParent<FirstPersonController>();
        Logger.Debug($"Setting equip object player to {parent.Name}");
        _equipObject.SetPlayer(parent);
        _currentEquipItem = item;
    }

    public void Unequip()
    {
        if (_currentEquipItem == null) return;

        _equipObject.QueueFree();
        _currentEquipItem = null;
        _equipObject = null;
    }
}