using Godot;

namespace OpenWorldSurvival.scripts;

[GlobalClass]
public partial class Item : Resource
{
    [Export] public Texture2D Icon;
    [Export] public int MaxStackSize = 20;
    [Export] public string Name;
    [Export] public PackedScene WorldItemScene;

    public virtual bool OnUse()
    {
        return true;
    }
}