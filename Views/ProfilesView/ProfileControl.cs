using Godot;
using System;

public partial class ProfileControl : Control
{
    [Export]
    public int Profile;

    [Export]
    public Button SelectButton;

    [Export]
    public Button DeleteButton;

    [Export]
    public Label ProfileNameLabel;

    [Export]
    public Label LastPlayedLabel;

    public event Action<int> OnProfileDeletePressed;
    public event Action<int> OnProfileSelected;

    public override void _Ready()
    {
        base._Ready();

        ProfileNameLabel.Text = $"Profile {Profile}";

        SelectButton.Pressed += ClickSelect;
        DeleteButton.Pressed += ClickDelete;
    }

    public void Load()
    {
        var data = GameProfileController.Instance.GetGameProfile(Profile);
        if (data.Deleted)
        {
            SetNoData();
        }
        else
        {
            SetData(data);
        }
    }

    private void SetData(GameSaveData data)
    {
        DeleteButton.Show();
        LastPlayedLabel.Show();
        LastPlayedLabel.Text = $"Last played: {data.DateTimeUpdated.ToString("dd:MM:yyyy HH:mm")}";
    }

    private void SetNoData()
    {
        DeleteButton.Hide();
        LastPlayedLabel.Hide();
    }

    private void ClickDelete()
    {
        OnProfileDeletePressed?.Invoke(Profile);
    }

    private void ClickSelect()
    {
        OnProfileSelected?.Invoke(Profile);
    }
}
