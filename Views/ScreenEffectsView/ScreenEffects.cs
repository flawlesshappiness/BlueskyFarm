using Godot;
using System.Collections;
using System.Collections.Generic;

public partial class ScreenEffects : Node3DScript
{
    public static ScreenEffects Instance { get; private set; }
    public static ScreenEffectsView View => _view ?? (_view = global::View.Get<ScreenEffectsView>());
    private static ScreenEffectsView _view;

    private Dictionary<string, Coroutine> _coroutines = new();

    private MultiFloatMin _radial_amount = new();
    private MultiFloatMin _gaussian_amount = new();
    private MultiFloatMin _distort_strength = new();
    private MultiFloatMin _fog_alpha = new();

    private static int _test;

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        RegisterDebugActions();
    }

    public void Clear()
    {
        // Clear coroutines
        foreach (var kvp in _coroutines)
        {
            Coroutine.Stop(kvp.Value);
        }
        _coroutines.Clear();

        // Clear floats
        _radial_amount.Clear();
        _gaussian_amount.Clear();
        _distort_strength.Clear();
        _fog_alpha.Clear();

        // Reset view
        View.Clear();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (View == null) return;
        View.RadialBlurAmount = _radial_amount.Value;
        View.GaussianBlurAmount = _gaussian_amount.Value;
        View.DistortStrength = _distort_strength.Value;
        View.Fog_Alpha = _fog_alpha.Value;
    }

    private void RegisterDebugActions()
    {
        var category = "ScreenEffects";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Test gaussian blur",
            Action = v => { AnimateGaussianBlur($"{nameof(ScreenEffects)}{_test++}", 20, 1, 1, 1); v.Close(); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Test radial blur",
            Action = v => { AnimateRadialBlur($"{nameof(ScreenEffects)}{_test++}", 0.01f, 1, 1, 1); v.Close(); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Test distort",
            Action = v => { AnimateDistort($"{nameof(ScreenEffects)}{_test++}", 0.05f, 1, 3, 1); v.Close(); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Reset",
            Action = v => { Clear(); v.Close(); }
        });
    }

    private Coroutine _AnimateMinValue(MultiFloatMin controller, string id, float value, float duration_in, float duration_on, float duration_out)
    {
        var cr = StartCoroutine(Cr, $"{nameof(_AnimateMinValue)}_{id}");
        _coroutines.Add(id, cr);
        return cr;

        IEnumerator Cr()
        {
            var start = controller.DefaultValue;
            var end = value;
            var curve = Curves.EaseInOutQuad;

            yield return LerpEnumerator.Lerp01(duration_in, f =>
            {
                controller.Set(id, Mathf.Lerp(start, end, curve.Evaluate(f)));
            });

            yield return new WaitForSeconds(duration_on);

            yield return LerpEnumerator.Lerp01(duration_out, f =>
            {
                controller.Set(id, Mathf.Lerp(end, start, curve.Evaluate(f)));
            });

            controller.Remove(id);
            _coroutines.Remove(id);
        }
    }

    public static Coroutine AnimateGaussianBlur(string id, float strength, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValue(Instance._gaussian_amount, id, strength, duration_in, duration_on, duration_out);

    public static Coroutine AnimateDistort(string id, float strength, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValue(Instance._distort_strength, id, strength, duration_in, duration_on, duration_out);

    public static Coroutine AnimateRadialBlur(string id, float strength, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValue(Instance._radial_amount, id, strength, duration_in, duration_on, duration_out);

    public static Coroutine AnimateFog(string id, float alpha, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValue(Instance._fog_alpha, id, alpha, duration_in, duration_on, duration_out);

    public static void SetGaussianBlur(string id, float strength) =>
        Instance._gaussian_amount.Set(id, strength);

    public static void RemoveGaussianBlur(string id) =>
        Instance._gaussian_amount.Remove(id);
}
