using Godot;
using System;
using System.Collections.Generic;

public partial class DebugCategoryControl : Control
{
    [Export]
    public DebugView View;

    [Export]
    public Label CategoryLabel;

    [Export]
    public Button ExpandButton;

    [Export]
    public Control ButtonsParent;

    [Export]
    public Button ActionButtonTemplate;

    [Export]
    public AudioStreamPlayer SfxButtonHover;

    [Export]
    public AudioStreamPlayer SfxButtonPress;

    public List<Button> ActionButtons { get; private set; } = new();

    public override void _Ready()
    {
        base._Ready();
        ButtonsParent.Hide();
        ActionButtonTemplate.Hide();

        ExpandButton.Pressed += () => SetExpanded(!ButtonsParent.Visible);
        SetExpanded(false);
    }

    public void CreateButton(string text, Action<DebugView> action)
    {
        var button = ActionButtonTemplate.Duplicate() as Button;
        button.SetParent(ButtonsParent);
        button.Show();

        button.Text = text;
        button.Pressed += () => action?.Invoke(View);
        button.Pressed += () => SfxButtonPress.Play();
        button.MouseEntered += () => SfxButtonHover.Play();

        ActionButtons.Add(button);
    }

    private void SetExpanded(bool expanded)
    {
        ButtonsParent.Visible = expanded;
        ExpandButton.Text = expanded ? "v" : ">";
    }
}
