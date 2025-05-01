using Godot;

namespace OpenWorldSurvival.FirstPersonController;

public partial class DebugPanel : PanelContainer
{
    private VBoxContainer _boxContainer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _boxContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer");
    }

    public void AddProperty(string title, string value, int order)
    {
        if (_boxContainer.GetType() != typeof(VBoxContainer)) return;
        var target = _boxContainer.FindChild(title, true, false);
        if (target == null)
        {
            var label = new Label();
            _boxContainer.AddChild(label);
            label.Name = title;
            label.Text = $"{title}: {value}";
        }
        else if (Visible)
        {
            var label = (Label)target;
            label.Text = $"{title}: {value}";
            _boxContainer.MoveChild(target, order);
        }
    }
}