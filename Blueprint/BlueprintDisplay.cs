using Godot;
using System;
using System.Collections;
using System.Linq;

public partial class BlueprintDisplay : Node3DScript
{
    [NodeName]
    public BlueprintDisplayCounter VegetableCounter;

    [NodeType]
    public Touchable Touchable;

    public event Action OnCancel;

    public override void _Ready()
    {
        base._Ready();
        Touchable.InteractableText = Tr("##CANCEL##");
        Touchable.OnTouched += Touched;
    }

    public void UpdateText(BlueprintCraftingData data)
    {
        if (data == null) return;

        UpdateCounter(VegetableCounter, data.Materials.FirstOrDefault(x => x.Type == ItemType.Vegetable));
    }

    private void UpdateCounter(BlueprintDisplayCounter counter, BlueprintCraftingMaterialData data)
    {
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