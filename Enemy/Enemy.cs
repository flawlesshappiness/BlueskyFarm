using Godot;
using System;
using System.Collections;
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
    public bool IsDebug { get; set; }
    protected string EnemyId => $"{EnemyName} {GetInstanceId()}";
    protected string EnemyCategory => $"{EnemyName} {TargetArea}";
    protected virtual string EnemyName => $"{Name}";
    public bool Spawned { get; private set; }
    protected MultiLock StateLock { get; private set; } = new MultiLock();
    protected string CurrentState { get; private set; }
    protected virtual string DefaultState { get; }
    protected Dictionary<string, Func<IEnumerator>> States { get; private set; } = new();
    protected Coroutine CurrentStateCoroutine { get; private set; }

    private Label _debug_info_label;

    public virtual void InitializeEnemy()
    {
        RegisterStates();
        RegisterDebugActions();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Debug.RemoveActions(EnemyId);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessDebugLabel();
    }

    protected virtual void RegisterDebugActions()
    {
        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyCategory,
            Text = "Show info",
            Action = v => ShowPosition()
        });

        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyCategory,
            Text = "Hide info",
            Action = v => HidePosition()
        });

        void ShowPosition()
        {
            _debug_info_label = GameView.Instance.CreateText(new CreateTextSettings
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
            if (_debug_info_label == null) return;
            _debug_info_label.QueueFree();
            _debug_info_label = null;
        }

        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyCategory,
            Text = "Respawn",
            Action = _ => Spawn()
        });

        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyCategory,
            Text = "Set state: Default",
            Action = _ => SetState(DefaultState)
        });
    }

    public bool HasPlayerLOS()
    {
        var origin = GlobalPosition.Add(y: 0.25f);
        var end = PlayerPosition.Add(y: 0.5f);
        var raycast_player = this.TryRaycast(origin, end, CollisionMaskHelper.Create(CollisionMaskType.Player), out var result_player);
        var raycast_world = this.TryRaycast(origin, PlayerPosition.Add(y: 0.5f), CollisionMaskHelper.Create(CollisionMaskType.World), out var result_world);

        return raycast_player && !raycast_world;
    }

    public void Spawn() => Spawn(IsDebug);
    public virtual void Spawn(bool debug)
    {
        Spawned = true;
        IsDebug = debug;
    }

    public virtual void Respawn()
    {
        Show();
        Spawn();
    }

    public virtual void Despawn()
    {
        Spawned = false;
        Hide();
        StopState();
    }

    protected virtual void RegisterStates()
    {
        // Register states here
        // RegisterState(state_name, StateCr_State);
    }

    protected void RegisterState(string state, Func<IEnumerator> enumerator)
    {
        if (!States.ContainsKey(state))
        {
            States.Add(state, enumerator);
        }
    }

    protected bool IsState(params string[] states)
    {
        return states.Any(x => x == CurrentState);
    }

    protected void SetState(string state)
    {
        if (StateLock.IsLocked) return;
        CurrentState = state;

        var id = "state";
        var enumerator = States.TryGetValue(state, out var _enumerator) ? _enumerator : null;
        if (enumerator == null)
        {
            Debug.LogError($"{EnemyName} unable to set state: {state}");
            return;
        }

        CurrentStateCoroutine = this.StartCoroutine(enumerator, id);
    }

    protected void StopState()
    {
        Coroutine.Stop(CurrentStateCoroutine);
    }

    protected IEnumerable<BasementRoomElement> GetRooms(Func<BasementRoomElement, bool> func = null)
    {
        return RoomElements
            .Where(x => !x.IsStart && x.AreaName == TargetArea && (func == null || func(x)));
    }

    private void ProcessDebugLabel()
    {
        if (_debug_info_label == null) return;
        _debug_info_label.Text = GetInfoString();
    }

    protected virtual string GetInfoString()
    {
        var state = Spawned ? CurrentState : "Despawned";
        return $"{EnemyName}\n{state}";
    }
}
