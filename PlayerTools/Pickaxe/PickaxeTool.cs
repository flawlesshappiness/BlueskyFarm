using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class PickaxeTool : Node3D
{
    [Export]
    public Area3D AreaContainer;

    [Export]
    public AnimationPlayer AnimationPlayer;

    private List<GodotObject> _bodies = new();

    public override void _Ready()
    {
        base._Ready();
        AreaContainer.BodyEntered += v => CallDeferred(nameof(BodyEntered), v);
        AreaContainer.BodyExited += v => CallDeferred(nameof(BodyExited), v);
        AnimationPlayer.AnimationFinished += AnimationFinished;
    }

    public void AnimateUse()
    {
        var has_container = HasContainer();
        var anim = has_container ? "swing_hit" : "swing_miss";
        AnimationPlayer.Play(anim);
        SetPlayerLocked(true);
    }

    public void AnimationHit()
    {
        BreakContainers();
    }

    public bool HasContainer()
    {
        return _bodies.Any(x => GetContainer(x) != null);
    }

    private void BreakContainers()
    {
        foreach (var go in _bodies)
        {
            BreakContainer(go);
        }
    }

    private void BreakContainer(GodotObject go)
    {
        var container = GetContainer(go);
        if (!IsInstanceValid(container)) return;

        container.Break();
    }

    private RockContainer GetContainer(GodotObject go)
    {
        if (!IsInstanceValid(go)) return null;

        var node = go as Node3D;
        if (!IsInstanceValid(node)) return null;

        var container = node.GetNodeInParents<RockContainer>();
        if (!IsInstanceValid(container)) return null;

        return container;
    }

    private void BodyEntered(GodotObject go)
    {
        _bodies.Add(go);
    }

    private void BodyExited(GodotObject go)
    {
        _bodies.Remove(go);
    }

    private void AnimationFinished(StringName animation)
    {
        SetPlayerLocked(false);
    }

    private void SetPlayerLocked(bool locked)
    {
        Player.Instance.MovementLock.SetLock(nameof(PickaxeTool), locked);
        Player.Instance.LookLock.SetLock(nameof(PickaxeTool), locked);
        Player.Instance.InteractLock.SetLock(nameof(PickaxeTool), locked);
        InventoryController.Instance.InventoryLock.SetLock(nameof(PickaxeTool), locked);
    }
}
