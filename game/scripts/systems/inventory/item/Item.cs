using Godot;
using OpenWorldSurvival.engine.core;

namespace OpenWorldSurvival.game.scripts.systems.inventory.item;

[GlobalClass]
public partial class Item : Resource
{
    [Export] public Texture2D Icon;
    [Export] public int MaxStackSize = 20;
    [Export] public string Name;
    [Export] public PackedScene WorldItemScene;

    public virtual bool OnUse(CharacterBody3D character)
    {
        Log.Debug("Item " + Name + " was used!");
        return false;
    }
}