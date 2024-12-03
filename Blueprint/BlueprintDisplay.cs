using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class BlueprintDisplay : Node3DScript
{
    [NodeName]
    public BlueprintDisplayCounter Counter1;

    [NodeName]
    public BlueprintDisplayCounter Counter2;

    [NodeName]
    public BlueprintDisplayCounter Counter3;

    [NodeName]
    public Sprite3D ResultIcon;

    [NodeType]
    public Touchable Touchable;

    public event Action OnCancel;

    private List<BlueprintDisplayCounter> _counters = new();

    public override void _Ready()
    {
        base._Ready();
        Touchable.InteractableText = Tr("##CANCEL##");
        Touchable.OnTouched += Touched;
        SetCancelEnabled(false);

        _counters = new() { Counter1, Counter2, Counter3 };
    }

    public void UpdateFromData(BlueprintCraftingData data)
    {
        if (data == null) return;

        var info = BlueprintController.Instance.GetInfo(data.Id);
        ResultIcon.Texture = info.ResultIcon;

        for (int i = 0; i < _counters.Count; i++)
        {
            var counter = _counters[i];
            var material = i < data.Materials.Count ? data.Materials[i] : null;
            if (material == null)
            {
                counter.Hide();
            }
            else
            {
                counter.Show();
                UpdateCounter(counter, material);
            }
        }
    }

    private void UpdateCounter(BlueprintDisplayCounter counter, BlueprintCraftingMaterialData data)
    {
        counter.SetType(data.Type);
        counter.SetValue(data.Max - data.Count);
    }

    private void Touched()
    {
        CancelCrafting();
    }

    public void SetCancelEnabled(bool enabled)
    {
        Touchable.SetEnabled(enabled);
    }

    public void CancelCrafting()
    {
        OnCancel?.Invoke();
    }

    public Coroutine AnimateShow()
    {
        return StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            Show();

            var duration = 0.25f;
            var curve = Curves.EaseOutBack;
            SoundController.Instance.Play("sfx_throw_light", GlobalPosition);
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                var t = curve.Evaluate(f);
                Scale = Vector3.One * Mathf.Lerp(0.01f, 1f, t);
            });
        }
    }

    public Coroutine AnimateHide()
    {
        return StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            var duration = 0.25f;
            var curve = Curves.EaseInBack;
            SoundController.Instance.Play("sfx_throw_light", GlobalPosition);
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                var t = curve.Evaluate(f);
                Scale = Vector3.One * Mathf.Lerp(1f, 0.01f, t);
            });

            Hide();
        }
    }
}