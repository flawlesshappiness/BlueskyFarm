using Godot;
using System;
using System.Collections.Generic;

public partial class CombinationSelector : NodeScript
{
    [Export]
    public int CombinationLength;

    public string CurrentCombination { get; private set; }

    public event Action<string> OnCombinationChanged;

    private IEnumerable<CombinationInput> _inputs;

    public override void _Ready()
    {
        base._Ready();
        InitializeInputs();
    }

    private void InitializeInputs()
    {
        _inputs = this.GetNodesInChildren<CombinationInput>();
        foreach (var input in _inputs)
        {
            input.OnTouched += () => TouchInput(input);
        }
    }

    private void TouchInput(CombinationInput input)
    {
        SetCombination(CurrentCombination + input.Input);
        SoundController.Instance.Play(input.SfxInput, new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = input.GlobalPosition,
        });
    }

    public void Clear()
    {
        SetCombination(string.Empty);
    }

    private void SetCombination(string combination)
    {
        Debug.TraceMethod(CurrentCombination);

        CurrentCombination = combination ?? string.Empty;
        CurrentCombination = CurrentCombination.Length > CombinationLength ? CurrentCombination.Substring(0, CombinationLength) : CurrentCombination;
        OnCombinationChanged?.Invoke(CurrentCombination);
    }
}
