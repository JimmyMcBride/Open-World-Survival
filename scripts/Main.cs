using Godot;

namespace OpenWorldSurvival.scripts;

public partial class Main : Node3D
{
    private AnimationTree _swordAnimationTree;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _swordAnimationTree = GetNode<AnimationTree>("Sword/AnimationTree");

        _swordAnimationTree.Active = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}