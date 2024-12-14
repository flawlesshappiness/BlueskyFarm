using System;
using System.Collections.Generic;
using System.Linq;

public partial class PuzzleCubeWall : Node3DScript
{
    private List<PuzzleCubeWallHole> _holes = new();
    private List<PuzzleCubeDisplay> _displays = new();
    private List<SequenceEntry> _target_sequence = new();

    public event Action OnSequenceCompleted;
    public event Action OnSequenceIncorrect;

    private class SequenceEntry
    {
        public PuzzleCube.Type Type { get; set; }
        public PuzzleCube.Color Color { get; set; }
    }

    public override void _Ready()
    {
        base._Ready();
        _holes = this.GetNodesInChildren<PuzzleCubeWallHole>();
        _displays = this.GetNodesInChildren<PuzzleCubeDisplay>();

        foreach (var hole in _holes)
        {
            hole.OnCubeChanged += CubeChanged;
        }

        SetRandomSequence();
    }

    public void SetRandomSequence()
    {
        _target_sequence.Clear();
        var types = Enum.GetValues(typeof(PuzzleCube.Type)).Cast<PuzzleCube.Type>().ToList();
        var colors = Enum.GetValues(typeof(PuzzleCube.Color)).Cast<PuzzleCube.Color>().ToList();
        colors.Remove(PuzzleCube.Color.Disabled);

        for (int i = 0; i < _holes.Count; i++)
        {
            var type = types.Random();
            var color = colors.Random();
            types.Remove(type);
            _target_sequence.Add(new SequenceEntry
            {
                Type = type,
                Color = color
            });
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < _target_sequence.Count; i++)
        {
            var display = _displays[i];
            var entry = _target_sequence[i];
            var alignment = PuzzleCube.Alignments.FirstOrDefault(x => x.Type == entry.Type);
            display.GlobalRotationDegrees = alignment.Rotation + GlobalRotationDegrees;
            display.SetColor(entry.Color);
        }
    }

    private void CubeChanged()
    {
        ValidateSequence();
    }

    private void ValidateSequence()
    {
        if (IsSequenceValid())
        {
            _holes.ForEach(x => { x.Disable(); x.SetCubeColorDisabled(); });
            SoundController.Instance.Play("sfx_puzzle_basement");
            OnSequenceCompleted?.Invoke();
        }
        else
        {
            OnSequenceIncorrect?.Invoke();
        }
    }

    private bool IsSequenceValid()
    {
        var seq = GetSequence();
        for (int i = 0; i < _target_sequence.Count; i++)
        {
            var entry = _target_sequence[i];
            var hole = _holes[i];

            if (entry.Type != hole.SelectedType) return false;
            if (entry.Color != hole.SelectedColor) return false;
        }

        return true;
    }

    private List<PuzzleCube.Type?> GetSequence()
    {
        return _holes.Select(x => x.SelectedType).ToList();
    }

    private void LogSequence(List<PuzzleCube.Type> seq)
    {
        var s = "";
        seq.ForEach(x => s += $"{x} ");
        Debug.Log(s);
    }
}
