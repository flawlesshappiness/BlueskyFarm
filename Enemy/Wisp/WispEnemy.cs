using Godot;
using System;
using System.Collections;
using System.Linq;

public partial class WispEnemy : NavEnemy
{
    [Export]
    public Node3D AnimNode;

    [Export]
    public Path3D Path;

    protected override string EnemyName => "WispEnemy";
    protected override string DefaultState => StateMove;

    private float _current_path_distance;
    private Vector3 _target_position;

    private const string StateMove = "Move";
    private const string StateHover = "Hover";

    private BasementRoomElement _current_room;

    protected override void Initialize()
    {
        base.Initialize();
        AnimNode.SetParent(GetParent());
        InitializeSfxLoop();
    }

    private void InitializeSfxLoop()
    {
        var info = SoundController.Instance.Collection.GetResource("sfx_wisp_tingle");
        var stream = info.AudioStreams.FirstOrDefault();
        var length = Convert.ToSingle(stream.GetLength());
        var rng = new RandomNumberGenerator();
        SoundController.Instance.Play("sfx_wisp_tingle", AnimNode, new SoundOverride
        {
            PlaybackPosition = rng.RandfRange(0, length)
        });
    }

    protected override void RegisterStates()
    {
        base.RegisterStates();
        RegisterState(StateMove, StateCr_Move);
        RegisterState(StateHover, StateCr_Hover);
    }

    public override void Spawn(bool debug)
    {
        base.Spawn(debug);

        if (debug)
        {

        }
        else
        {
            AnimNode.Show();
            _current_room = GetFurthestRoomElementToPlayer();
            GlobalPosition = _current_room.Room.GlobalPosition;
            SetState(StateMove);
        }
    }

    public override void Despawn()
    {
        base.Despawn();
        AnimNode.Hide();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        ProcessPath();
        ProcessPosition();
    }

    private void ProcessPath()
    {
        var length = Path.Curve.GetBakedLength();
        var speed = 0.5f * GameTime.DeltaTime;
        _current_path_distance = (_current_path_distance + speed) % length;
        _target_position = Path.GlobalPosition + Path.Curve.SampleBaked(offset: _current_path_distance);
    }

    private void ProcessPosition()
    {
        AnimNode.GlobalPosition = AnimNode.GlobalPosition.Lerp(_target_position, 0.5f * GameTime.DeltaTime);
    }

    IEnumerator StateCr_Move()
    {
        _current_room = GetConnnectedNeighbours(_current_room).ToList().Random() ?? _current_room;
        var position = GetGravePositionInRoom(_current_room);
        Agent.TargetPosition = position;

        while (true)
        {
            if (Agent.IsNavigationFinished())
            {
                SetState(StateHover);
            }

            yield return null;
        }
    }

    IEnumerator StateCr_Hover()
    {
        var rng = new RandomNumberGenerator();
        yield return new WaitForSeconds(rng.RandfRange(4, 10));
        SetState(StateMove);
    }

    private Vector3 GetGravePositionInRoom(BasementRoomElement room)
    {
        var graves = room.Room
            .GetNodesInChildren<GraveContainer>()
            .Where(x => x.IsVisibleInTree());

        var grave = graves.ToList().Random();

        if (grave == null)
        {
            return GetRandomPositionInRoom(room.Room);
        }
        else
        {
            return grave.GlobalPosition;
        }
    }
}
