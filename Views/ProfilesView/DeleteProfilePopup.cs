using Godot;
using System;

public partial class DeleteProfilePopup : Control
{
    [Export]
    public Label TitleLabel;

    [Export]
    public Button ButtonDelete;

    [Export]
    public Button ButtonCancel;

    public event Action OnDelete;

    public override void _Ready()
    {
        base._Ready();
        ButtonDelete.Pressed += ClickDelete;
        ButtonCancel.Pressed += ClickCancel;
    }

    public void UpdateTitle(int profile)
    {
        TitleLabel.Text = $"Delete Profile {profile}?";
    }

    private void ClickDelete()
    {
        OnDelete?.Invoke();
        Hide();
    }

    private void ClickCancel()
    {
        Hide();
    }
}
