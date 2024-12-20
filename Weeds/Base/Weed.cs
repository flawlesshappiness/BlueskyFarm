using Godot;
using System;
using System.Collections;

public partial class Weed : Node3DScript
{
    [Export]
    public bool ToolRequired;

    [NodeType]
    public Touchable Touchable;

    public event Action OnWeedCut;

    private bool _is_cut;

    public override void _Ready()
    {
        base._Ready();
        Touchable.OnTouched += Touched;
    }

    private void Touched()
    {
        if (_is_cut) return;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return Player.Instance.WaitForProgress(0.5f, Touchable);
            Cut();
        }
    }

    public virtual void Cut()
    {
        if (_is_cut) return;
        _is_cut = true;

        Touchable.Disable();

        OnWeedCut?.Invoke();
    }
}
