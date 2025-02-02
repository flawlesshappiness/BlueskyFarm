using Godot;
using System;

[GlobalClass]
public partial class PlayerArea : Area3D
{
    public event Action<Player> OnPlayerEntered, OnPlayerExited;

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

        PlayerEntered(player);
    }

    private void OnBodyExited(GodotObject go)
    {
        var node = go as Node3D;
        if (!IsInstanceValid(node)) return;

        var player = node as Player;
        if (!IsInstanceValid(player)) return;

        PlayerExited(player);
    }

    protected virtual void PlayerEntered(Player player)
    {
        OnPlayerEntered?.Invoke(player);
    }

    protected virtual void PlayerExited(Player player)
    {
        OnPlayerExited?.Invoke(player);
    }
}
