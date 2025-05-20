using Godot;
using System;

public partial class ClockRock : Node3D
{
    [Export]
    public CrushableRock Crushable;

    [Export]
    public AnimationPlayer AnimationPlayer;

    public bool Toggled { get; private set; }

    public Action<bool> OnToggled;

    public override void _Ready()
    {
        base._Ready();
        Crushable.OnCrushed += Crushed;
    }

    private void Crushed()
    {
        Toggle();
    }

    private void Toggle()
    {
        Toggled = !Toggled;

        var anim = Toggled ? "activate" : "deactivate";
        AnimationPlayer.Play(anim);

        OnToggled?.Invoke(Toggled);
    }
}
