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

        if (ToolRequired)
        {
            SoundController.Instance.Play("sfx_weed_cut", Touchable.GlobalPosition);

            GameView.Instance.CreateText(new CreateTextSettings
            {
                Id = "tool_required_" + GetInstanceId(),
                Text = "##TOOL_REQUIRED##",
                Target = Touchable,
                Offset = new Vector3(0, 0.2f, 0),
                Duration = 5.0f,
                Shake_Duration = 0.4f,
                Shake_Frequency = 0.04f,
                Shake_Strength = 10f,
                Shake_Dampening = 0.9f,
            });
            return;
        }

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
