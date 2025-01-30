using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class EggClusterEnemy : NavEnemy
{
    [Export]
    public Array<Node3D> ClusterTemplates;

    protected override string EnemyName => "EggCluster";

    private List<Node3D> _clusters = new();

    private BasementRoomElement _current_room;

    protected override void Initialize()
    {
        base.Initialize();
        Spawn();
    }

    public override void Spawn(bool debug)
    {
        base.Spawn(debug);

        if (debug)
        {
            CreateClusters();
        }
        else
        {
            var room = GetRooms()
            .Where(x => x.Info.ValidGroundHeights != null && x.Info.ValidGroundHeights.Count > 0)
            .ToList()
            .Random();

            if (room == null)
            {
                // Do nothing
            }
            else
            {
                _current_room = room;
                GlobalPosition = room.Room.GlobalPosition;
                CreateClusters();
            }
        }
    }

    public override void Despawn()
    {
        base.Despawn();
        ClearClusters();
    }

    private void ClearClusters()
    {
        foreach (var cluster in _clusters)
        {
            cluster.QueueFree();
        }

        _clusters.Clear();
    }

    private void CreateClusters()
    {
        var count = 60;
        for (int i = 0; i < count; i++)
        {
            CreateCluster();
        }
    }

    private Node3D CreateCluster()
    {
        var cluster = ClusterTemplates.ToList().Random().Duplicate() as Node3D;
        cluster.SetParent(this);

        var position = GetValidEggPosition();
        cluster.GlobalPosition = position;

        _clusters.Add(cluster);

        cluster.Enable();
        return cluster;
    }

    private Vector3 GetValidEggPosition()
    {
        var position = Vector3.Zero;
        var valid = false;
        while (!valid)
        {
            var room_position = GetRandomPositionAroundMe();
            position = NavigationServer3D.MapGetClosestPoint(GetWorld3D().NavigationMap, room_position).Add(y: -Agent.PathHeightOffset);
            valid = _current_room.Info.ValidGroundHeights.Any(y => position.Y == y);
        }

        return position;
    }

    private Vector3 GetRandomPositionAroundMe()
    {
        var rng = new RandomNumberGenerator();
        var room_size = BasementRoom.ROOM_SIZE;
        var min = -0.4f;
        var max = 0.4f;
        var x = rng.RandfRange(min, max);
        var z = rng.RandfRange(min, max);
        return GlobalPosition + new Vector3(x, 0, z) * room_size;
    }
}
