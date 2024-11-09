using Godot;
using System;
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

    public bool IsFull => Data.Uses >= MaxUses;
    public bool IsEmpty => Data.Uses == 0;

    private bool _using;
    private float _slosh_speed_min;
    private float _slosh_speed_max;
    private List<GodotObject> _bodies = new();

    private const float SPEED_SLOSH_LOW_MIN = 2f;
    private const float SPEED_SLOSH_LOW_MAX = 4f;
    private const float SPEED_SLOSH_MED_MIN = 4f;
    private const float SPEED_SLOSH_MED_MAX = 7f;
    private const float SPEED_SLOSH_HIGH_MIN = 5f;
    private const float SPEED_SLOSH_HIGH_MAX = 10f;


    public override void _Ready()
    {
        base._Ready();

        Area.BodyEntered += v => CallDeferred(nameof(BodyEntered), v);
        Area.BodyExited += v => CallDeferred(nameof(BodyExited), v);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var fdelta = Convert.ToSingle(delta);
        PhysicsProcess_Slosh(fdelta);
    }

    private void PhysicsProcess_Slosh(float delta)
    {
        if (IsEmpty) return;

        var vel = RigidBody.LinearVelocity.Length();

        _slosh_speed_max = Mathf.Max(_slosh_speed_max, vel);
        _slosh_speed_min =
            _slosh_speed_max > SPEED_SLOSH_HIGH_MAX ? SPEED_SLOSH_HIGH_MIN :
            _slosh_speed_max > SPEED_SLOSH_MED_MAX ? SPEED_SLOSH_MED_MIN :
            _slosh_speed_max > SPEED_SLOSH_LOW_MAX ? SPEED_SLOSH_LOW_MIN :
            -1;

        if (vel < _slosh_speed_min)
        {
            PlaySloshSFX(_slosh_speed_max);
            _slosh_speed_max = 0;
        }
    }

    private void PlaySloshSFX(float speed)
    {
        var power = GetPowerString(speed);
        var sfx_name = $"sfx_watering_can_slosh_{power}";

        SoundController.Instance.Play(sfx_name, GlobalPosition);

        string GetPowerString(float speed)
        {
            return
                speed > SPEED_SLOSH_HIGH_MAX ? "high" :
                speed > SPEED_SLOSH_MED_MAX ? "med" :
                speed > SPEED_SLOSH_LOW_MAX ? "low" :
                "";
        }
    }

    public override void Use()
    {
        base.Use();

        if (Data.Uses > 0)
        {
            AnimateUse();
        }
        else
        {
            var view = View.Get<GameView>();
            view.DisplayText("The watering can is empty");
        }
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
            Data.Uses--;

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
