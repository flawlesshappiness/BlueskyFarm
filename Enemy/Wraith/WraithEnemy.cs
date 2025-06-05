using Godot;
using System.Collections;
using System.Linq;

public partial class WraithEnemy : NavEnemy
{
    [Export]
    public GpuParticles3D ps_body;

    [Export]
    public GpuParticles3D ps_spawn;

    [Export]
    public GpuParticles3D ps_despawn;

    [Export]
    public Node3D Model;

    [Export]
    public Node3D Eyes;

    [Export]
    public SoundInfo SfxSpawn;

    [Export]
    public SoundInfo SfxLoop;

    [Export]
    public PackedScene KillBoxPrefab;

    protected override string EnemyName => "Wraith";
    protected override string DefaultState => StateIdle;
    private const string StateIdle = "Idle";

    private const string StateSpawn = "Spawn";
    private const string StateChase = "Chase";

    private BasementRoomElement _current_element;
    private AudioStreamPlayer3D _asp_loop;

    private bool _is_seen;
    private bool _look_at_blur_enabled;

    private static bool kill_box_spawned;
    private static WraithKillBox kill_box;

    public override void _Ready()
    {
        base._Ready();
        _asp_loop = SfxLoop.Play(this, new SoundOverride
        {
            Volume = -80f
        });

        CreateKillBox();
    }

    protected override void RegisterStates()
    {
        base.RegisterStates();
        RegisterState(StateIdle, StateCr_Idle);
        RegisterState(StateSpawn, StateCr_Spawn);
        RegisterState(StateChase, StateCr_Chase);
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

            SetState(StateIdle);
        }
    }

    public override void Despawn()
    {
        base.Despawn();
        RemoveEffects();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessLookAtPlayer();
        ProcessLookAtEffects();
    }

    private void ProcessLookAtPlayer()
    {
        TurnTowardsDirection(-DirectionToPlayer);
    }

    private void ProcessLookAtEffects()
    {
        if (!_look_at_blur_enabled) return;

        var max_distance = BasementRoom.ROOM_SIZE * 0.6f;
        var t_distance = Mathf.Clamp(1f - DistanceToPlayer / max_distance, 0, 1);
        var t_look = Player.Instance.GetLookAtT(GlobalPosition);
        var t = t_distance * t_look;
        var blur = Mathf.Lerp(0f, 0.02f, t);
        ScreenEffects.SetRadialBlur(EnemyId, blur);

        var hbfreq = Mathf.Lerp(1f, 0.25f, t);
        ScreenEffects.SetHeartbeatFrequency(EnemyId, hbfreq);

        if (!_is_seen && t > 0.25f)
        {
            _is_seen = true;
            SoundController.Instance.Play("sfx_horror_boom");
            SoundController.Instance.Play("sfx_horror_chord");
        }
    }

    private void RemoveEffects()
    {
        _is_seen = false;
        _look_at_blur_enabled = false;
        ScreenEffects.RemoveRadialBlur(EnemyId);
        ScreenEffects.RemoveHeartbeatFrequency(EnemyId);
        ScreenEffects.AnimateRadialBlurOut(EnemyId, 4f);
    }

    protected override void OnVelocityComputed(Vector3 v)
    {
        Move(v);
    }

    IEnumerator StateCr_Idle()
    {
        // Hide
        ps_body.Emitting = false;
        Model.Hide();

        // Find room with grave
        BasementRoomElement grave_room = null;
        GraveContainer grave = null;
        while (grave == null)
        {
            grave_room = GetRooms().ToList().Random();
            grave = grave_room.Room.GetNodesInChildren<GraveContainer>().ToList().Random();
            yield return null;
        }

        // Move to grave
        GlobalPosition = grave.GlobalPosition;
        Agent.TargetPosition = grave.GlobalPosition;
        _current_element = grave_room;

        // Wait for player
        while (DistanceToPlayer > BasementRoom.ROOM_SIZE * 0.5f)
        {
            yield return null;
        }

        // Spawn
        SetState(StateSpawn);
    }

    IEnumerator StateCr_Spawn()
    {
        SfxSpawn.Play(this);
        _asp_loop.Fade(2f, SfxLoop.Volume);
        ps_spawn.Emitting = true;
        yield return new WaitForSeconds(2f);

        _look_at_blur_enabled = true;

        // Show body
        Model.Show();
        ps_body.Emitting = true;

        // Chase player
        SetState(StateChase);
    }

    IEnumerator StateCr_Chase()
    {
        // Move towards player
        var time_end = GameTime.Time + 10f;
        var distance_end = BasementRoom.ROOM_SIZE;
        while (GameTime.Time < time_end && DistanceToPlayer < distance_end)
        {
            if (DistanceToPlayer < 2)
            {
                KillPlayer();
                break;
            }

            Agent.TargetPosition = PlayerPosition;
            yield return null;
        }

        Agent.TargetPosition = GlobalPosition;

        // Disappear
        Eyes.Hide();
        ps_body.Emitting = false;
        ps_despawn.Emitting = true;
        _asp_loop.FadeOut(4f);
        yield return new WaitForSeconds(1f);
        Model.Hide();
        Eyes.Show();

        RemoveEffects();

        yield return new WaitForSeconds(30f);

        SetState(StateIdle);
    }

    private void KillPlayer()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            Player.Instance.Interrupt();
            Player.SetLocked(nameof(WraithEnemy), true);
            kill_box.TeleportPlayer();

            SoundController.Instance.Play("sfx_horror_boom");

            kill_box.SfxBreathe.Play();
            kill_box.SfxBreathe.VolumeDb = -10f;

            yield return new WaitForSeconds(3.0f);
            yield return kill_box.SfxBreathe.Fade(0.5f, 0f);

            Player.SetLocked(nameof(WraithEnemy), false);
            GameScene.Current.KillPlayer();
        }
    }

    private void CreateKillBox()
    {
        if (kill_box_spawned) return;
        kill_box_spawned = true;

        kill_box = KillBoxPrefab.Instantiate<WraithKillBox>();
        kill_box.SetParent(GetParent());
        kill_box.GlobalPosition = new Vector3(0, -100, 0);
    }

    public static void KillBoxRemoved()
    {
        kill_box = null;
        kill_box_spawned = false;
    }
}
