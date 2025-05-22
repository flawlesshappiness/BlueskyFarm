using Godot;
using System;

public partial class LightTrigger : Decal
{
    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public ItemArea ItemArea;

    public bool IsToggleable { get; set; } = true;
    public bool IsToggled { get; private set; }

    public Action<bool> OnToggle;

    public override void _Ready()
    {
        base._Ready();
        ItemArea.OnItemEntered += ItemEntered;
    }

    private void ItemEntered(Item item)
    {
        Toggle();
    }

    private void Toggle()
    {
        if (!IsToggleable) return;

        IsToggled = !IsToggled;
        var anim = IsToggled ? "activate" : "deactivate";
        AnimationPlayer.Play(anim);

        OnToggle?.Invoke(IsToggled);
    }
}
