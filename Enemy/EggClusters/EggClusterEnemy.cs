using Godot;
using Godot.Collections;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class EggClusterEnemy : NavEnemy
{
    [Export]
    public Array<Node3D> ClusterTemplates;

    protected override string EnemyName => "EggCluster";

    private List<Node3D> _cluster_templates;
    private List<Node3D> _clusters = new();

    private BasementRoomElement _current_room;
    private bool _clusters_spawned;

    public override void InitializeEnemy()
    {
        base.InitializeEnemy();
        _cluster_templates = ClusterTemplates.ToList();

        foreach (var cluster in _cluster_templates)
        {
            cluster.Disable();
        }
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
                // Failed to get a valid room
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
            cluster.Disable();
        }
    }

    private void CreateClusters()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(0.1f);

            if (!_clusters_spawned)
            {
                _clusters_spawned = true;
                var count = 60;
                for (int i = 0; i < count; i++)
                {
                    CreateCluster();
                }
            }

            UpdateClusterPositions();
        }
    }

    private void UpdateClusterPositions()
    {
        foreach (var cluster in _clusters)
        {
            var position = GetValidEggPosition();
            cluster.GlobalPosition = position;

            if (position == Vector3.Zero)
            {
                cluster.Disable();
            }

            Debug.Log(position);
        }
    }

    private Node3D CreateCluster()
    {
        var cluster = _cluster_templates.Random().Duplicate() as Node3D;
        cluster.SetParent(this);

        _clusters.Add(cluster);

        cluster.Enable();
        return cluster;
    }

    private Vector3 GetValidEggPosition()
    {
        var position = Vector3.Zero;
        var valid = false;
        var safety = 5;
        while (!valid && safety > 0)
        {
            safety--;
            var room_position = GetRandomPositionAroundMe();
            var map = GetWorld3D().NavigationMap;
            position = NavigationServer3D.MapGetClosestPoint(map, room_position);
            valid = _current_room.Info.ValidGroundHeights.Any(y => position.Y == y);

            if (position == Vector3.Zero) break;

            position -= Vector3.Up * Agent.PathHeightOffset;
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
