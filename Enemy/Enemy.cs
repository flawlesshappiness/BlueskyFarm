using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Enemy : Node3DScript
{
    public string TargetArea { get; set; }
    protected Player Player => global::Player.Instance;
    protected Vector3 PlayerPosition => Player.GlobalPosition;
    protected float DistanceToPlayer => GlobalPosition.DistanceTo(PlayerPosition);
    protected Vector3 DirectionToPlayer => GlobalPosition.DirectionTo(PlayerPosition);
    protected IEnumerable<BasementRoomElement> RoomElements => BasementController.Instance.CurrentBasement.Grid.Elements;
    protected bool IsDebug { get; private set; }
    protected string EnemyId => $"{EnemyName} {GetInstanceId()}";
    protected string EnemyCategory => $"{EnemyName} {TargetArea}";
    protected virtual string EnemyName => $"{Name}";

    private Label _show_position_label;

    protected override void Initialize()
    {
        base.Initialize();
        RegisterDebugActions();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Debug.RemoveActions(EnemyId);
    }

    protected virtual void RegisterDebugActions()
    {
        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyCategory,
            Text = "Show position",
            Action = v => ShowPosition()
        });

        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyCategory,
            Text = "Hide position",
            Action = v => HidePosition()
        });

        void ShowPosition()
        {
            _show_position_label = GameView.Instance.CreateText(new CreateTextSettings
            {
                Id = EnemyId,
                Text = EnemyName,
                Duration = -1,
                Target = this,
                Offset = new Vector3(0, 1, 0)
            });
        }

        void HidePosition()
        {
            if (_show_position_label == null) return;
            _show_position_label.QueueFree();
            _show_position_label = null;
        }
    }

    public bool CanSeePlayer()
    {
        if (this.TryRaycast(GlobalPosition.Add(y: 0.25f), PlayerPosition.Add(y: 0.5f), CollisionMaskHelper.Create(CollisionMaskType.Player), out var result))
        {
            var collider = result.Collider as Node;
            var player = collider.GetNodeInParents<FirstPersonController>();
            return IsInstanceValid(player);
        }

        return false;
    }

    public virtual void DebugSpawn()
    {
        IsDebug = true;
    }

    protected IEnumerable<BasementRoomElement> GetRooms(Func<BasementRoomElement, bool> func = null)
    {
        return RoomElements
            .Where(x => !x.IsStart && x.AreaName == TargetArea && (func == null || func(x)));
    }
}
