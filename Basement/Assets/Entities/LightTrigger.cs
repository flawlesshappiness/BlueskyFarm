using Godot;
using System;

public partial class LightTrigger : Decal
{
    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public ItemArea ItemArea;

    public bool IsToggleable { get; set; } = true;

    public Action<bool> OnToggle;

    private bool activated;

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

        activated = !activated;
        var anim = activated ? "activate" : "deactivate";
        AnimationPlayer.Play(anim);

        OnToggle?.Invoke(activated);
    }
}
