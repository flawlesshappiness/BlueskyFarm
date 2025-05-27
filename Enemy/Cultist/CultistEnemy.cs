using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;
using System.Linq;

public partial class CultistEnemy : NavEnemy
{
    [Export]
    public float HuntSpeed;

    [Export]
    public Node3D Model;

    [Export]
    public Node3D CameraTarget;

    [Export]
    public Marker3D CameraMarker_Attack;

    [Export]
    public SoundInfo SfxWalk;

    [Export]
    public SoundInfo SfxRun;

    [Export]
    public AnimationStateMachine AnimationStateMachine;

    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public AudioStreamPlayer3D SfxAlert;

    [Export]
    public SoundInfo SfxPlayerDeath;

    protected override string EnemyName => "Cultist";
    protected override string DefaultState => StatePatrol;

    private BoolParameter _param_moving;
    private BoolParameter _param_running;
    private BoolParameter _param_attacking;
    private AnimationState _anim_startled;

    private BasementRoomElement _current_element;

    private const string StateWait = "Wait";
    private const string StatePatrol = "Patrol";
    private const string StateHunt = "Hunt";
    private const string StateAttack = "Attack";

    private float PlayerMaxDist => Player.Instance.IsRunning ? 10f : 5f;
    private bool PlayerTooClose => DistanceToPlayer < PlayerMaxDist && HasPlayerLOS();
    private float PlayerLOSDist => 12f;
    private float PlayerLOSAngle => 15f;
    private bool PlayerInsideViewAngle => AngleToPlayer < PlayerLOSAngle;
    private bool PlayerInsideViewDist => DistanceToPlayer < PlayerLOSDist;
    private bool CanSeePlayer => HasPlayerLOS() && PlayerInsideViewDist && PlayerInsideViewAngle;

    public override void InitializeEnemy()
    {
        base.InitializeEnemy();
        InitializeAnimations();
    }

    private void InitializeAnimations()
    {
        AnimationPlayer.PlaybackDefaultBlendTime = 0.15f;
        AnimationStateMachine.Initialize(AnimationPlayer);

        var anim_idle = AnimationStateMachine.CreateAnimation("Armature|Idle_Cult", true);
        var anim_walk = AnimationStateMachine.CreateAnimation("Armature|Walk_Cult", true);
        var anim_run = AnimationStateMachine.CreateAnimation("Armature|Run_Cult", true);
        _anim_startled = AnimationStateMachine.CreateAnimation("Armature|Startled_Cult", false);
        var anim_attack = AnimationStateMachine.CreateAnimation("Armature|Attack_Cult", true);

        _param_moving = new BoolParameter("moving", false);
        _param_running = new BoolParameter("running", false);
        _param_attacking = new BoolParameter("attacking", false);

        AnimationStateMachine.Connect(anim_walk.Node, anim_idle.Node, _param_moving.WhenFalse());
        AnimationStateMachine.Connect(anim_run.Node, anim_idle.Node, _param_moving.WhenFalse());
        AnimationStateMachine.Connect(_anim_startled.Node, anim_idle.Node);

        AnimationStateMachine.Connect(anim_idle.Node, anim_walk.Node, _param_moving.WhenTrue(), _param_running.WhenFalse());
        AnimationStateMachine.Connect(anim_run.Node, anim_walk.Node, _param_moving.WhenTrue(), _param_running.WhenFalse());

        AnimationStateMachine.Connect(anim_idle.Node, anim_run.Node, _param_moving.WhenTrue(), _param_running.WhenTrue());
        AnimationStateMachine.Connect(anim_walk.Node, anim_run.Node, _param_moving.WhenTrue(), _param_running.WhenTrue());

        AnimationStateMachine.Connect(anim_idle.Node, anim_attack.Node, _param_attacking.WhenTrue());
        AnimationStateMachine.Connect(anim_walk.Node, anim_attack.Node, _param_attacking.WhenTrue());
        AnimationStateMachine.Connect(anim_run.Node, anim_attack.Node, _param_attacking.WhenTrue());

        AnimationStateMachine.Start(anim_idle.Node);
    }

    protected override void RegisterStates()
    {
        base.RegisterStates();
        RegisterState(StatePatrol, PatrolState);
        RegisterState(StateWait, WaitState);
        RegisterState(StateHunt, HuntState);
        RegisterState(StateAttack, AttackState);
    }

    public override void Spawn(bool debug)
    {
        base.Spawn(debug);

        if (debug)
        {

        }
        else
        {
            _current_element = GetFurthestRoomElementToPlayer();
            var spawn = _current_element.Room.GetNodeInChildren<Node3D>("EnemySpawn");
            GlobalPosition = spawn.GlobalPosition;

            SetState(StatePatrol);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process_Animations();
        Process_Attack();
        Process_LOS();
        Process_Speed();
    }

    private void Process_Animations()
    {
        if (!Spawned) return;
        _param_moving.Set(!Agent.IsNavigationFinished());
    }

    private void Process_Attack()
    {
        if (IsState(StateHunt))
        {
            if (DistanceToPlayer < 3)
            {
                SetState(StateAttack);
            }
        }
    }

    private void Process_LOS()
    {
        if (IsState(StatePatrol) || IsState(StateWait))
        {
            if (PlayerTooClose || CanSeePlayer)
            {
                SetState(StateHunt);
            }
        }
    }

    private void Process_Speed()
    {
        CurrentSpeed = IsState(StateHunt) ? HuntSpeed : MoveSpeed;
    }

    private IEnumerator WaitState()
    {
        _param_running.Set(false);

        ScreenEffects.AnimateRadialBlurOut(EnemyId, 2f);
        ScreenEffects.RemoveHeartbeatFrequency(EnemyId);

        Agent.TargetPosition = GlobalPosition;

        var time = GameTime.Time + 3f;
        while (GameTime.Time < time)
        {
            yield return null;
        }

        SetState(StatePatrol);
    }

    private IEnumerator PatrolState()
    {
        _param_running.Set(false);
        CurrentSpeed = MoveSpeed;

        _current_element = GetClosestRoomElements(x => x != _current_element).FirstOrDefault();
        Agent.TargetPosition = _current_element.Room.EnemyMarker.GlobalPosition;
        Model.Show();

        while (true)
        {
            if (Agent.IsNavigationFinished())
            {
                SetState(StateWait);
            }

            yield return null;
        }
    }

    private IEnumerator HuntState()
    {
        StopNavigation();
        StartFacingPlayer();
        SfxAlert.Play();

        yield return new WaitForSeconds(1f);

        SoundController.Instance.Play("sfx_horror_boom");
        SoundController.Instance.Play("sfx_horror_chord");
        ScreenEffects.SetHeartbeatFrequency(EnemyId, 0.75f);
        ScreenEffects.AnimateRadialBlurIn(EnemyId, 0.005f, 2f);

        yield return new WaitForSeconds(1f);

        StopFacingPlayer();

        Agent.TargetPosition = PlayerPosition;
        while (IsMoving)
        {
            if (!CanSeePlayer)
            {
                // Move to last seen location
            }
            else
            {
                Agent.TargetPosition = PlayerPosition;
            }

            yield return null;
        }

        SetState(StateWait);
    }

    private IEnumerator AttackState()
    {
        StopNavigation();
        StartFacingPlayer();

        Player.Interrupt();
        Player.MovementLock.AddLock(EnemyId);
        Player.LookLock.AddLock(EnemyId);

        var start_pos = ScreenEffects.View.Camera.GlobalPosition;
        var end_pos = CameraMarker_Attack.GlobalPosition;
        var end_rot = CameraMarker_Attack.GlobalRotationDegrees.WrappedEulerAngles();
        var start_rot = ScreenEffects.View.Camera.GlobalRotationDegrees.ClosestEulerAngle(end_rot);
        var curve = Curves.EaseOutQuad;

        Player.CameraRagdoll.Freeze = true;
        CameraTarget.GlobalPosition = start_pos;
        CameraTarget.GlobalRotationDegrees = start_rot;
        _param_attacking.Set(true);

        ScreenEffects.SetHeartbeatFrequency(EnemyId, 0.5f);
        ScreenEffects.AnimateRadialBlurIn(EnemyId, 0.02f, 2.0f);
        ScreenEffects.AnimateCameraShakeIn(EnemyId, 0.05f, 1.0f);

        SfxPlayerDeath.Play();

        ScreenEffects.View.SetCameraTarget(CameraTarget);
        yield return LerpEnumerator.Lerp01(1.0f, f =>
        {
            var t = curve.Evaluate(f);
            CameraTarget.GlobalPosition = start_pos.Lerp(end_pos, t);
            CameraTarget.GlobalRotationDegrees = start_rot.Lerp(end_rot, t);
        });

        yield return new WaitForSeconds(2.0f);

        GameScene.Current.KillPlayer();

        ScreenEffects.AnimateRadialBlurOut(EnemyId, 0);
        ScreenEffects.AnimateCameraShakeOut(EnemyId, 0);
        ScreenEffects.RemoveHeartbeatFrequency(EnemyId);

        Player.MovementLock.RemoveLock(EnemyId);
        Player.LookLock.RemoveLock(EnemyId);
    }

    private IEnumerator StateCr_Flee()
    {
        Agent.TargetPosition = GlobalPosition;
        AnimationStateMachine.SetCurrentState(_anim_startled.Node);
        SoundController.Instance.Play("sfx_horror_chord");
        SoundController.Instance.Play("sfx_horror_boom");

        yield return new WaitForSeconds(0.5f);

        _param_running.Set(true);

        // Run towards player
        var timeout = GameTime.Time + 3f;
        while (DistanceToPlayer > 2f && GameTime.Time < timeout)
        {
            Agent.TargetPosition = PlayerPosition;
            yield return null;
        }

        RagdollPlayer();

        // Run away
        _current_element = GetFurthestRoomElementToPlayer();
        Agent.TargetPosition = _current_element.Room.GlobalPosition;

        while (true)
        {
            if (Agent.IsNavigationFinished())
            {
                //SetState(StateRespawn);
            }

            yield return null;
        }
    }

    private void RagdollPlayer()
    {
        Coroutine.Start(Cr, $"{nameof(CultistEnemy)}_Ragdoll");
        IEnumerator Cr()
        {
            ScreenEffects.AnimateGaussianBlur(EnemyId, 15, 0.2f, 3f, 5f);
            ScreenEffects.AnimateHeartbeatFrequency(EnemyId, 0.75f, 0, 2f, 5f);

            var v_ragdoll = DirectionToPlayer.Normalized() * 2f;
            yield return Player.Instance.RagdollCameraAndPickUp(v_ragdoll, 3f);
        }
    }

    private IEnumerator StateCr_Respawn()
    {
        _param_running.Set(true);

        while (true)
        {
            Model.Hide();
            yield return new WaitForSeconds(30);
            Spawn(false);
        }
    }

    public void PlayWalkSfx()
    {
        SoundController.Instance.Play(SfxWalk, GlobalPosition.Add(y: 0.5f), new SoundOverride
        {
            Volume = -20,
            Distance = SoundDistance.Far
        });
    }

    public void PlayRunSfx()
    {
        SoundController.Instance.Play(SfxRun, GlobalPosition.Add(y: 0.5f), new SoundOverride
        {
            Volume = -20,
            Distance = SoundDistance.Far
        });
    }
}
