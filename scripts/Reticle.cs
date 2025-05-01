using Godot;

namespace OpenWorldSurvival.scripts;

public partial class Reticle : CenterContainer
{
    [Export] public Color Color = Colors.White;
    [Export] public float Radius = 1.0f;

    public override void _Draw()
    {
        DrawCircle(new Vector2(0, 0), Radius, Color);
    }
}