using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class EggClusterEnemy : NavEnemy
{
    [NodeName]
    public Node3D Clusters;

    private List<Node3D> _cluster_templates = new();
    private List<Node3D> _clusters = new();

    public override void _Ready()
    {
        base._Ready();

        InitializeClusters();
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (IsDebug)
        {
            SpawnDebug();
        }
        else
        {
            Spawn();
        }
    }

    private void InitializeClusters()
    {
        foreach (var child in Clusters.GetChildren())
        {
            var n3 = child as Node3D;
            if (!IsInstanceValid(n3)) continue;

            n3.Disable();
            _cluster_templates.Add(n3);
        }
    }

    private void Spawn()
    {
        var room = GetRooms()
            .Where(x => !x.Info.HasUnevenGround)
            .ToList()
            .Random();

        if (room == null)
        {
            // Do nothing
        }
        else
        {
            GlobalPosition = room.Room.GlobalPosition;
            CreateClusters();
        }
    }

    private void SpawnDebug()
    {
        CreateClusters();
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
        var cluster = _cluster_templates.Random().Duplicate() as Node3D;
        cluster.SetParent(this);

        var room_position = GetRandomPositionAroundMe();
        var position = NavigationServer3D.MapGetClosestPoint(GetWorld3D().NavigationMap, room_position).Add(y: -Agent.PathHeightOffset);
        cluster.GlobalPosition = position;

        _clusters.Add(cluster);

        cluster.Enable();
        return cluster;
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
