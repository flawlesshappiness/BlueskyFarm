using Godot;
using System;
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
    private MultiFloatMin _squeeze_x_strength = new();
    private MultiFloatMin _squeeze_y_strength = new();
    private MultiFloatMin _fog_alpha = new();
    private float _camera_fov = 75f;
    private float _camera_fov_default = 75f;
    private Vector3 _camera_offset;
    private MultiFloatSum _camera_offset_forward = new();

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
        _squeeze_x_strength.Clear();
        _squeeze_y_strength.Clear();
        _fog_alpha.Clear();
        _camera_fov = _camera_fov_default;
        _camera_offset = Vector3.Zero;
        _camera_offset_forward.Clear();

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
        View.SqueezeXAmount = _squeeze_x_strength.Value;
        View.SqueezeYAmount = _squeeze_y_strength.Value;
        View.Fog_Alpha = _fog_alpha.Value;
        View.Camera.Fov = Mathf.Lerp(View.Camera.Fov, _camera_fov, 5f * (float)delta);
        View.Camera_Offset = View.Camera_Offset.Lerp(_camera_offset, 5f * (float)delta);
        View.Camera_Offset_Forward = Mathf.Lerp(View.Camera_Offset_Forward, _camera_offset_forward.Value, 5f * (float)delta);
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
            Text = "Test squeeze",
            Action = v => { AnimateSqueezeX($"{nameof(ScreenEffects)}{_test++}", 0.5f, 5, 1, 5); v.Close(); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Test fov",
            Action = v => { AnimateFov($"{nameof(ScreenEffects)}{_test++}", 90f, 5, 1, 5); v.Close(); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Test camera offset",
            Action = v => { _AnimateValue(f => SetCameraOffset(Vector3.Right * 2 * f), $"{nameof(ScreenEffects)}{_test++}", 0, 1, 2, 0, 2); v.Close(); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Test camera offset forward",
            Action = v => { _AnimateValue(f => SetCameraOffsetForward($"{nameof(ScreenEffects)}{_test}", 2 * f), $"{nameof(ScreenEffects)}{_test}", 0, 1, 2, 0, 2); v.Close(); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Test dolly zoom",
            Action = v =>
            {
                _AnimateValue(f => SetCameraOffset(Vector3.Forward * 2 * f), $"{nameof(ScreenEffects)}{_test++}", 0, 1, 2, 0, 2);
                AnimateFov($"{nameof(ScreenEffects)}{_test++}", 120f, 2, 0, 2);
                v.Close();
            }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Reset",
            Action = v => { Clear(); v.Close(); }
        });
    }

    private void AddCoroutine(string id, Coroutine coroutine)
    {
        if (_coroutines.ContainsKey(id))
        {
            _coroutines[id] = coroutine;
        }
        else
        {
            _coroutines.Add(id, coroutine);
        }
    }

    private Coroutine _AnimateMinValue(MultiFloatMin controller, string id, float value, float duration_in, float duration_on, float duration_out)
    {
        var cr = StartCoroutine(Cr, $"{nameof(_AnimateMinValue)}_{id}");
        AddCoroutine(id, cr);
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

    private Coroutine _AnimateValue(Action<float> action, string id, float start, float end, float duration_in, float duration_on, float duration_out)
    {
        var cr = StartCoroutine(Cr, $"{nameof(_AnimateValue)}_{id}");
        AddCoroutine(id, cr);
        return cr;

        IEnumerator Cr()
        {
            var curve = Curves.EaseInOutQuad;

            yield return LerpEnumerator.Lerp01(duration_in, f =>
            {
                var value = Mathf.Lerp(start, end, curve.Evaluate(f));
                action(value);
            });

            yield return new WaitForSeconds(duration_on);

            yield return LerpEnumerator.Lerp01(duration_out, f =>
            {
                var value = Mathf.Lerp(end, start, curve.Evaluate(f));
                action(value);
            });

            _coroutines.Remove(id);
        }
    }

    public static Coroutine AnimateGaussianBlur(string id, float strength, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValue(Instance._gaussian_amount, id, strength, duration_in, duration_on, duration_out);

    public static Coroutine AnimateDistort(string id, float strength, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValue(Instance._distort_strength, id, strength, duration_in, duration_on, duration_out);

    public static Coroutine AnimateSqueezeX(string id, float strength, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValue(Instance._squeeze_x_strength, id, strength, duration_in, duration_on, duration_out);

    public static Coroutine AnimateSqueezeY(string id, float strength, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValue(Instance._squeeze_y_strength, id, strength, duration_in, duration_on, duration_out);

    public static Coroutine AnimateRadialBlur(string id, float strength, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValue(Instance._radial_amount, id, strength, duration_in, duration_on, duration_out);

    public static Coroutine AnimateFog(string id, float alpha, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValue(Instance._fog_alpha, id, alpha, duration_in, duration_on, duration_out);

    public static Coroutine AnimateFov(string id, float fov, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateValue(v => Instance._camera_fov = v, id, Instance._camera_fov_default, fov, duration_in, duration_on, duration_out);

    public static void SetGaussianBlur(string id, float strength) =>
        Instance._gaussian_amount.Set(id, strength);

    public static void RemoveGaussianBlur(string id) =>
        Instance._gaussian_amount.Remove(id);

    public static void SetCameraOffset(Vector3 offset) =>
        Instance._camera_offset = offset;

    public static void SetCameraOffsetForward(string id, float distance) =>
        Instance._camera_offset_forward.Set(id, distance);

    public static void RemoveCameraOffsetForward(string id) =>
        Instance._camera_offset_forward.Remove(id);
}
