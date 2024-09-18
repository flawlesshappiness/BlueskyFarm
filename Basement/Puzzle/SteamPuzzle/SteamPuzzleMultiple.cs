using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class SteamPuzzleMultiple : SteamPuzzle
{
    private IEnumerable<InteractableLever> _levers;

    public override void _Ready()
    {
        base._Ready();
        InitializeLevers();

        StartSteam();
    }

    private void InitializeLevers()
    {
        _levers = this.GetNodesInChildren<InteractableLever>();
        _levers.ForEach(x => x.OnStateChanged += i => LeverStateChanged(x, i));
    }

    private void LeverStateChanged(InteractableLever lever, int i)
    {
        var count_lever_state = 0;
        var count_levers = _levers.Count();

        _levers.ForEach(x => count_lever_state += x.CurrentState);

        var ps_steam = lever.GetNodesInChildren<GpuParticles3D>();
        ps_steam.ForEach(x => x.Emitting = i == 0);

        var sfx_steam = lever.GetNodesInChildren<AudioStreamPlayer3D>();
        sfx_steam.ForEach(x => x.Playing = i == 0);

        if (count_lever_state == count_levers)
        {
            StopSteam();
        }
        else
        {
            StartSteam();
        }
    }
}
