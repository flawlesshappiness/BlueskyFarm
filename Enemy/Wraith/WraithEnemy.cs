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

    protected override string EnemyName => "Wraith";
    protected override string DefaultState => StateIdle;
    private const string StateIdle = "Idle";

    private const string StateSpawn = "Spawn";
    private const string StateChase = "Chase";

    private BasementRoomElement _current_element;
    private AudioStreamPlayer3D _asp_loop;

    public override void _Ready()
    {
        base._Ready();
        _asp_loop = SfxLoop.Play(this, new SoundOverride
        {
            Volume = -80f
        });
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

    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessLookAtPlayer();
    }

    private void ProcessLookAtPlayer()
    {
        TurnTowardsDirection(-DirectionToPlayer);
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
        _asp_loop.FadeIn(2f, SfxLoop.Volume);
        ps_spawn.Emitting = true;
        yield return new WaitForSeconds(2f);

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

        yield return new WaitForSeconds(10f);

        SetState(StateIdle);
    }

    private void KillPlayer()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            Player.Instance.RagdollCamera(DirectionToPlayer.Normalized() * 2);

            yield return new WaitForSeconds(2.0f);

            GameScene.Current.KillPlayer();
        }
    }
}
