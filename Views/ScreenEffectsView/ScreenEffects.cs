using Godot;
using System.Collections;

public partial class ScreenEffects : Node3DScript
{
    public static ScreenEffects Instance { get; private set; }
    public static ScreenEffectsView View => _view ?? (_view = global::View.Get<ScreenEffectsView>());
    private static ScreenEffectsView _view;

    private MultiFloatMin _radial_amount = new();
    private MultiFloatMin _gaussian_amount = new();
    private MultiFloatMin _distort_strength = new();
    private MultiFloatMin _fog_alpha = new();

    private static int _test;

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        RegisterDebugActions();
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
            Action = v => { AnimateDistort($"{nameof(ScreenEffects)}{_test++}", 0.05f, 0.3f, new Vector2(0.3f, 0.1f), 1, 3, 1); v.Close(); }
        });
    }

    public static Coroutine AnimateGaussianBlur(string id, float strength, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateGaussianBlur(id, strength, duration_in, duration_on, duration_out);

    private Coroutine _AnimateGaussianBlur(string id, float strength, float duration_in, float duration_on, float duration_out)
    {
        Debug.Log(id);
        return StartCoroutine(Cr, id);
        IEnumerator Cr()
        {
            var start = 0f;
            var end = strength;

            yield return LerpEnumerator.Lerp01(duration_in, f =>
            {
                _gaussian_amount.Set(id, Mathf.Lerp(start, end, f));
            });

            yield return new WaitForSeconds(duration_on);

            start = strength;
            end = 0f;

            yield return LerpEnumerator.Lerp01(duration_out, f =>
            {
                _gaussian_amount.Set(id, Mathf.Lerp(start, end, f));
            });

            _gaussian_amount.Remove(id);
        }
    }

    public static Coroutine AnimateDistort(string id, float strength, float speed, Vector2 displacement, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateDistort(id, strength, speed, displacement, duration_in, duration_on, duration_out);

    private Coroutine _AnimateDistort(string id, float strength, float speed, Vector2 displacement, float duration_in, float duration_on, float duration_out)
    {
        return StartCoroutine(Cr, id);
        IEnumerator Cr()
        {
            View.DistortSpeed = speed;
            View.DistortDisplacement = displacement;

            var start = 0f;
            var end = strength;

            yield return LerpEnumerator.Lerp01(duration_in, f =>
            {
                _distort_strength.Set(id, Mathf.Lerp(start, end, f));
            });

            yield return new WaitForSeconds(duration_on);

            start = strength;
            end = 0f;

            yield return LerpEnumerator.Lerp01(duration_out, f =>
            {
                _distort_strength.Set(id, Mathf.Lerp(start, end, f));
            });

            _distort_strength.Remove(id);
        }
    }

    public static Coroutine AnimateRadialBlur(string id, float strength, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateRadialBlur(id, strength, duration_in, duration_on, duration_out);

    private Coroutine _AnimateRadialBlur(string id, float strength, float duration_in, float duration_on, float duration_out)
    {
        return StartCoroutine(Cr, id);
        IEnumerator Cr()
        {
            var start = 0f;
            var end = strength;

            yield return LerpEnumerator.Lerp01(duration_in, f =>
            {
                _radial_amount.Set(id, Mathf.Lerp(start, end, f));
            });

            yield return new WaitForSeconds(duration_on);

            start = strength;
            end = 0f;

            yield return LerpEnumerator.Lerp01(duration_out, f =>
            {
                _radial_amount.Set(id, Mathf.Lerp(start, end, f));
            });

            _radial_amount.Remove(id);
        }
    }

    public static Coroutine AnimateFog(string id, float alpha, Color color, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateFog(id, alpha, color, duration_in, duration_on, duration_out);

    private Coroutine _AnimateFog(string id, float alpha, Color color, float duration_in, float duration_on, float duration_out)
    {
        return StartCoroutine(Cr, id);
        IEnumerator Cr()
        {
            var start = 0f;
            var end = alpha;

            yield return LerpEnumerator.Lerp01(duration_in, f =>
            {
                _fog_alpha.Set(id, Mathf.Lerp(start, end, f));
            });

            yield return new WaitForSeconds(duration_on);

            start = alpha;
            end = 0f;

            yield return LerpEnumerator.Lerp01(duration_out, f =>
            {
                _fog_alpha.Set(id, Mathf.Lerp(start, end, f));
            });

            _fog_alpha.Remove(id);
        }
    }
}
