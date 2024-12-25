using Godot;
using System.Collections.Generic;

public partial class Enemy : Node3DScript
{
    protected Player Player => global::Player.Instance;
    protected Vector3 PlayerPosition => Player.GlobalPosition;
    protected float DistanceToPlayer => GlobalPosition.DistanceTo(PlayerPosition);
    protected Vector3 DirectionToPlayer => GlobalPosition.DirectionTo(PlayerPosition);
    protected IEnumerable<BasementRoomElement> RoomElements => BasementController.Instance.CurrentBasement.Grid.Elements;
    protected bool IsDebug { get; private set; }
    protected string DebugCategory => $"{Name} {GetInstanceId()}";

    private Label _show_position_label;

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
    }

    private void RegisterDebugActions()
    {
        Debug.RegisterAction(new DebugAction
        {
            Category = DebugCategory,
            Text = "Show position",
            Action = v => ShowPosition()
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = DebugCategory,
            Text = "Hide position",
            Action = v => HidePosition()
        });

        void ShowPosition()
        {
            _show_position_label = GameView.Instance.CreateText(new CreateTextSettings
            {
                Id = DebugCategory,
                Text = DebugCategory,
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
}
