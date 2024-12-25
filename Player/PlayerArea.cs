using Godot;
using System;

[GlobalClass]
public partial class PlayerArea : Area3D
{
    public event Action<Player> PlayerEntered, PlayerExited;

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += b => CallDeferred(nameof(OnBodyEntered), b);
        BodyExited += b => CallDeferred(nameof(OnBodyExited), b);
    }

    private void OnBodyEntered(GodotObject go)
    {
        var node = go as Node3D;
        if (!IsInstanceValid(node)) return;

        var player = node as Player;
        if (!IsInstanceValid(player)) return;

        PlayerEntered?.Invoke(player);
    }

    private void OnBodyExited(GodotObject go)
    {
        var node = go as Node3D;
        if (!IsInstanceValid(node)) return;

        var player = node as Player;
        if (!IsInstanceValid(player)) return;

        PlayerExited?.Invoke(player);
    }
}
