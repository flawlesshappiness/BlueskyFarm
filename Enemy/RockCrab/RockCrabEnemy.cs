using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;
using System.Linq;

public partial class RockCrabEnemy : NavEnemy
{
    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public AnimationStateMachine AnimationStateMachine;

    [Export]
    public Marker3D HitPosition;

    protected override string DefaultState => "idle";

    private bool IsMoving => !Agent.IsNavigationFinished();
    private bool PlayerIsClose => DistanceToPlayer < 3;

    private BoolParameter _param_moving;
    private BoolParameter _param_hiding;
    private TriggerParameter _param_slamming;
    private TriggerParameter _param_eating;

    private BasementRoomElement _current_room;
    private string _current_animation;

    public override void InitializeEnemy()
    {
        base.InitializeEnemy();
        AnimationPlayer.AnimationStarted += AnimationStarted;
        InitializeAnimations();
    }

    private void InitializeAnimations()
    {
        AnimationStateMachine.Initialize(AnimationPlayer);

        var anim_idle = AnimationStateMachine.CreateAnimation("Armature|idle", true);
        var anim_eating = AnimationStateMachine.CreateAnimation("Armature|idle_eating", false);
        var anim_slamming = AnimationStateMachine.CreateAnimation("Armature|idle_slamming", false);
        var anim_walking = AnimationStateMachine.CreateAnimation("Armature|walking_right", true);
        var anim_to_hide = AnimationStateMachine.CreateAnimation("Armature|to_hide", false);
        var anim_from_hide = AnimationStateMachine.CreateAnimation("Armature|from_hide", false);

        _param_moving = new BoolParameter("moving", false);
        _param_hiding = new BoolParameter("hiding", false);
        _param_slamming = new TriggerParameter("slamming");
        _param_eating = new TriggerParameter("eating");

        AnimationStateMachine.Connect(anim_idle, anim_walking, _param_moving.WhenTrue());
        AnimationStateMachine.Connect(anim_walking, anim_idle, _param_moving.WhenFalse());
        AnimationStateMachine.Connect(anim_idle, anim_eating, _param_eating.WhenTriggered());
        AnimationStateMachine.Connect(anim_idle, anim_slamming, _param_slamming.WhenTriggered());

        AnimationStateMachine.Connect(anim_slamming, anim_idle);
        AnimationStateMachine.Connect(anim_eating, anim_idle);

        AnimationStateMachine.Connect(anim_slamming, anim_to_hide, _param_hiding.WhenTrue());
        AnimationStateMachine.Connect(anim_eating, anim_to_hide, _param_hiding.WhenTrue());
        AnimationStateMachine.Connect(anim_idle, anim_to_hide, _param_hiding.WhenTrue());
        AnimationStateMachine.Connect(anim_to_hide, anim_from_hide, _param_hiding.WhenFalse());
        AnimationStateMachine.Connect(anim_from_hide, anim_idle);

        AnimationStateMachine.Start(anim_idle.Node);
    }

    protected override void RegisterStates()
    {
        base.RegisterStates();
        RegisterState("idle", CrState_Idle);
        RegisterState("move", CrState_Moving);
        RegisterState("hide", CrState_Hiding);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process_Parameters();
    }

    private void Process_Parameters()
    {
        if (!Spawned) return;

        _param_moving.Set(IsMoving);

        if (PlayerIsClose && !IsState("hide"))
        {
            SetState("hide");
        }
    }

    protected override void OnVelocityComputed(Vector3 v)
    {
        if (_current_animation == "Armature|walking_right")
        {
            base.OnVelocityComputed(v);
        }
    }

    private void AnimationStarted(StringName name)
    {
        _current_animation = name.ToString();
    }

    public override void Spawn(bool debug)
    {
        base.Spawn(debug);

        if (debug)
        {

        }
        else
        {
            _current_room = GetRooms().ToList().Random();
            GlobalPosition = _current_room.Room.EnemyMarker.GlobalPosition;
            SetState(DefaultState);
        }
    }

    private IEnumerator CrState_Idle()
    {
        _param_eating.Untrigger();
        _param_slamming.Untrigger();

        var rng = new RandomNumberGenerator();
        var repeats = rng.RandiRange(1, 3);

        yield return new WaitForSeconds(rng.RandfRange(1f, 3f));

        for (int i = 0; i < repeats; i++)
        {
            if (rng.Randf() < 0.5f)
            {
                _param_slamming.Trigger();
                yield return new WaitForSeconds(1.5f);
            }

            _param_eating.Trigger();
            yield return new WaitForSeconds(1.5f);
        }

        SetState("move");
    }

    private IEnumerator CrState_Moving()
    {
        var position = GetRandomPositionInRoom(_current_room.Room);
        Agent.TargetPosition = position;

        while (IsMoving)
        {
            yield return null;
        }

        SetState("idle");
    }

    private IEnumerator CrState_Hiding()
    {
        Agent.TargetPosition = GlobalPosition;
        _param_hiding.Set(true);

        while (PlayerIsClose)
        {
            while (PlayerIsClose)
            {
                yield return null;
            }

            yield return new WaitForSeconds(3f);
        }

        _param_hiding.Set(false);

        SetState("move");
    }

    public void PlayHitPS()
    {
        Particle.PlayOneShot("ps_rock_crab_hit", HitPosition.GlobalPosition);
    }
}
