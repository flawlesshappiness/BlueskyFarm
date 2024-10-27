using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class WateringCan : Item
{
    [Export]
    public string UseAnimation;

    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeType]
    public Area3D Area;

    private bool _using;
    private List<GodotObject> _bodies = new();

    public override void _Ready()
    {
        base._Ready();

        Area.BodyEntered += v => CallDeferred(nameof(BodyEntered), v);
        Area.BodyExited += v => CallDeferred(nameof(BodyExited), v);
    }

    public override void Use()
    {
        base.Use();
        AnimateUse();
    }

    private void AnimateUse()
    {
        if (_using) return;

        StartCoroutine(Cr, "use");
        IEnumerator Cr()
        {
            _using = true;
            var animation = AnimationPlayer.GetAnimation(UseAnimation);
            AnimationPlayer.Play(UseAnimation);
            SetPlayerLocked(true);

            yield return new WaitForSeconds(animation.Length * 0.6f);

            WaterClosest();

            yield return new WaitForSeconds(animation.Length * 0.2f);

            SetPlayerLocked(false);

            yield return new WaitForSeconds(animation.Length * 0.2f);
            _using = false;
        }
    }

    private void SetPlayerLocked(bool locked)
    {
        var player = FirstPersonController.Instance;

        if (locked)
        {
            player.MovementLock.AddLock(nameof(WateringCan));
            player.LookLock.AddLock(nameof(WateringCan));
            player.InteractLock.AddLock(nameof(WateringCan));
        }
        else
        {
            player.MovementLock.RemoveLock(nameof(WateringCan));
            player.LookLock.RemoveLock(nameof(WateringCan));
            player.InteractLock.RemoveLock(nameof(WateringCan));
        }
    }

    private void WaterClosest()
    {
        var my_position = new Vector3(Area.GlobalPosition.X, 0, Area.GlobalPosition.Y);

        var closest = _bodies
            .Select(x => x as Node)
            .Where(x => IsInstanceValid(x))
            .Select(x => x.GetNodeInParents<Waterable>())
            .Where(x => IsInstanceValid(x))
            .OrderBy(x => my_position.DistanceTo(new Vector3(x.GlobalPosition.X, 0, x.GlobalPosition.Y)))
            .FirstOrDefault();

        if (!IsInstanceValid(closest)) return;

        closest.Water();
    }

    private void BodyEntered(GodotObject go)
    {
        _bodies.Add(go);
    }

    private void BodyExited(GodotObject go)
    {
        _bodies.Remove(go);
    }
}
