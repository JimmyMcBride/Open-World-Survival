using Godot;

namespace OpenWorldSurvival.game.scenes.modules.player;

public partial class Reticle : CenterContainer
{
    private Polygon2D _dot;
    private Color _dotColor = Colors.White;
    private int _dotSize = 1;

    public override void _Ready()
    {
        _dot = GetNode<Polygon2D>("dot");
    }

    // Called every frame. 'Delta' has been the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Visible) UpdateReticleSettings();
    }

    private void UpdateReticleSettings()
    {
        if (_dot.GetType() != typeof(Polygon2D)) return;

        var dotScale = _dot.Scale;
        dotScale.X = _dotSize;
        dotScale.Y = _dotSize;
        _dot.Scale = dotScale;
        _dot.Color = _dotColor;
    }
}