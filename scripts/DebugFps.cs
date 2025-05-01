using Godot;

namespace OpenWorldSurvival.scripts;

public partial class DebugFps : Label
{
    public override void _Process(double delta)
    {
        Text = $"FPS: {Engine.GetFramesPerSecond()}";
    }
}