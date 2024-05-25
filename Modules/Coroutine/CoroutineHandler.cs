using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CoroutineHandler : Node
{
    public static CoroutineHandler Instance => Singleton.TryGet<CoroutineHandler>(out var instance) ? instance : Create();

    private Dictionary<Guid, Coroutine> _coroutines = new();

    public static CoroutineHandler Create() =>
        Singleton.GetOrCreate<CoroutineHandler>($"{Paths.Modules}/Coroutine/{nameof(CoroutineHandler)}");

    public override void _Ready()
    {
        base._Ready();
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        foreach (var coroutine in _coroutines.Values.ToList())
        {
            coroutine.UpdateFrame();

            if (coroutine.HasCompleted)
            {
                RemoveCoroutine(coroutine);
            }
        }
    }

    public void AddCoroutine(Coroutine coroutine)
    {
        if (_coroutines.ContainsKey(coroutine.Id))
        {
            GD.PushWarning($"Attempted to add an existing coroutine with id: {coroutine.Id}");
            return;
        }

        coroutine.Id = Guid.NewGuid();
        _coroutines.Add(coroutine.Id, coroutine);
    }

    public void RemoveCoroutine(Coroutine coroutine)
    {
        _coroutines.Remove(coroutine.Id);
    }
}
