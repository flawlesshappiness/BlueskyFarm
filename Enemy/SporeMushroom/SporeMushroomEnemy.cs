using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class SporeMushroomEnemy : Enemy
{
    private List<Node3D> _clusters = new();
    private List<SpawnPosition> _spawn_positions = new();

    private Coroutine _cr_spawn;

    private SpawnPosition _current_cluster_position;

    private class SpawnPosition
    {
        public BasementRoomElement Element { get; set; }
        public SporeMushroomCluster Node { get; set; } = new();
    }

    public override void _Ready()
    {
        base._Ready();
        InitializeClusters();
        BeginSpawning();
    }

    private void InitializeClusters()
    {
        foreach (var element in BasementController.Instance.CurrentBasement.Grid.Elements)
        {
            var nodes = element.Room.GetNodesInChildren<SporeMushroomCluster>();

            foreach (var node in nodes)
            {
                var position = new SpawnPosition
                {
                    Element = element,
                    Node = node
                };

                position.Node.Hide();
                _spawn_positions.Add(position);
            }
        }
    }

    private SpawnPosition GetNextClusterPosition()
    {
        if (_current_cluster_position == null)
        {
            return _spawn_positions
                .Random();
        }
        else
        {
            var valid_elements = new List<BasementRoomElement> { _current_cluster_position.Element };
            valid_elements.AddRange(_current_cluster_position.Element.Connections);

            var valid_positions = _spawn_positions
                .Where(x => valid_elements.Contains(x.Element))
                .Where(x => !x.Node.Visible)
                .OrderBy(x => x.Node.GlobalPosition.DistanceTo(_current_cluster_position.Node.GlobalPosition));

            var valid_position = valid_positions.FirstOrDefault();

            return valid_position;
        }
    }

    private void SpawnNextCluster()
    {
        var next = GetNextClusterPosition();

        if (next == null || _current_cluster_position == next)
        {
        }
        else
        {
            _current_cluster_position = next;
            _current_cluster_position.Node.Show();
            _current_cluster_position.Node.AnimateAppear();
        }
    }

    private void BeginSpawning()
    {
        var rng = new RandomNumberGenerator();
        var min_time_between_spawns = 5f;
        var max_time_between_spawns = 15f;

        _cr_spawn = Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            while (true)
            {
                SpawnNextCluster();

                var wait_duration = rng.RandfRange(min_time_between_spawns, max_time_between_spawns);
                yield return new WaitForSeconds(wait_duration);
            }
        }
    }
}
