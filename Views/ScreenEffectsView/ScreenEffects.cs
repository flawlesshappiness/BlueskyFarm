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
    private MultiFloatMin _camera_shake_strength = new();
    private MultiFloatMax _heartbeat_frequency = new(1);
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
        _heartbeat_frequency.Clear();
        _camera_fov = _camera_fov_default;
        _camera_offset = Vector3.Zero;
        _camera_offset_forward.Clear();
        _camera_shake_strength.Clear();

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
        View.Camera.Fov = Mathf.Lerp(View.Camera.Fov, _camera_fov, 40f * (float)delta);
        View.Camera_Offset = View.Camera_Offset.Lerp(_camera_offset, 40f * (float)delta);
        View.Camera_Offset_Forward = Mathf.Lerp(View.Camera_Offset_Forward, _camera_offset_forward.Value, 40f * (float)delta);
        View.Camera_Shake_Strength = _camera_shake_strength.Value;
    }

    private void RegisterDebugActions()
    {
        var category = "SCREEN EFFECTS";

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
            Text = "Test camera shake",
            Action = v => { AnimateCameraShake($"{nameof(ScreenEffects)}{_test++}", 1, 1, 1, 1); v.Close(); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Start heartbeat",
            Action = v => { StartHeartbeat(); v.Close(); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Animate heartbeat",
            Action = v => { AnimateHeartbeatFrequency($"{nameof(ScreenEffects)}{_test++}", 0.25f, 0, 0, 10f); v.Close(); }
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

    private Coroutine _AnimateMinValueIn(MultiFloatMin controller, string id, float value, float duration_in)
    {
        var cr = this.StartCoroutine(Cr, id);
        AddCoroutine(id, cr);
        return cr;

        IEnumerator Cr()
        {
            var start = controller.Value;
            var end = value;
            var curve = Curves.EaseInOutQuad;

            yield return LerpEnumerator.Lerp01(duration_in, f =>
            {
                controller.Set(id, Mathf.Lerp(start, end, curve.Evaluate(f)));
            });

            _coroutines.Remove(id);
        }
    }

    private Coroutine _AnimateMinValueOut(MultiFloatMin controller, string id, float value, float duration_out)
    {
        var cr = this.StartCoroutine(Cr, id);
        AddCoroutine(id, cr);
        return cr;

        IEnumerator Cr()
        {
            var start = controller.Value;
            var end = value;
            var curve = Curves.EaseInOutQuad;

            yield return LerpEnumerator.Lerp01(duration_out, f =>
            {
                controller.Set(id, Mathf.Lerp(start, end, curve.Evaluate(f)));
            });

            controller.Remove(id);
            _coroutines.Remove(id);
        }
    }

    private Coroutine _AnimateMinValueInOut(MultiFloatMin controller, string id, float value, float duration_in, float duration_on, float duration_out)
    {
        var cr = this.StartCoroutine(Cr, id);
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
        var cr = this.StartCoroutine(Cr, id);
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

    // GAUSSIAN BLUR //
    /// <summary>
    /// Value range: 0 - 20
    /// </summary>
    public static Coroutine AnimateGaussianBlur(string id, float value, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValueInOut(Instance._gaussian_amount, $"{nameof(AnimateGaussianBlur)}_{id}", value, duration_in, duration_on, duration_out);

    /// <summary>
    /// Value range: 0 - 20
    /// </summary>
    public static Coroutine AnimateGaussianBlurIn(string id, float value, float duration) =>
        Instance._AnimateMinValueIn(Instance._gaussian_amount, $"{nameof(AnimateGaussianBlur)}_{id}", value, duration);

    public static Coroutine AnimateGaussianBlurOut(string id, float duration) =>
        Instance._AnimateMinValueOut(Instance._gaussian_amount, $"{nameof(AnimateGaussianBlur)}_{id}", 0, duration);

    /// <summary>
    /// Value range: 0 - 20
    /// </summary>
    public static void SetGaussianBlur(string id, float strength) =>
        Instance._gaussian_amount.Set(id, strength);

    public static void RemoveGaussianBlur(string id) =>
        Instance._gaussian_amount.Remove(id);

    // DISTORT //
    /// <summary>
    /// Value range: 0 - 0.05f
    /// </summary>
    public static Coroutine AnimateDistort(string id, float value, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValueInOut(Instance._distort_strength, $"{nameof(AnimateDistort)}_{id}", value, duration_in, duration_on, duration_out);

    /// <summary>
    /// Value range: 0 - 0.05f
    /// </summary>
    public static Coroutine AnimateDistortIn(string id, float value, float duration) =>
        Instance._AnimateMinValueIn(Instance._distort_strength, $"{nameof(AnimateDistort)}_{id}", value, duration);

    public static Coroutine AnimateDistortOut(string id, float duration) =>
        Instance._AnimateMinValueOut(Instance._distort_strength, $"{nameof(AnimateDistort)}_{id}", 0, duration);

    // SQUEEZE //
    public static Coroutine AnimateSqueezeX(string id, float value, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValueInOut(Instance._squeeze_x_strength, $"{nameof(AnimateSqueezeX)}_{id}", value, duration_in, duration_on, duration_out);

    public static Coroutine AnimateSqueezeY(string id, float value, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValueInOut(Instance._squeeze_y_strength, $"{nameof(AnimateSqueezeY)}_{id}", value, duration_in, duration_on, duration_out);

    // RADIAL BLUR //
    /// <summary>
    /// Value range: 0 - 0.02f
    /// </summary>
    public static Coroutine AnimateRadialBlur(string id, float value, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValueInOut(Instance._radial_amount, $"{nameof(AnimateRadialBlur)}_{id}", value, duration_in, duration_on, duration_out);

    /// <summary>
    /// Value range: 0 - 02f
    /// </summary>
    public static Coroutine AnimateRadialBlurIn(string id, float value, float duration) =>
        Instance._AnimateMinValueIn(Instance._radial_amount, $"{nameof(AnimateRadialBlur)}_{id}", value, duration);

    public static Coroutine AnimateRadialBlurOut(string id, float duration) =>
        Instance._AnimateMinValueOut(Instance._radial_amount, $"{nameof(AnimateRadialBlur)}_{id}", 0, duration);

    /// <summary>
    /// Value range: 0 - 0.02f
    /// </summary>
    public static void SetRadialBlur(string id, float strength) =>
        Instance._radial_amount.Set(id, strength);

    public static void RemoveRadialBlur(string id) =>
        Instance._radial_amount.Remove(id);

    // FOG //
    public static Coroutine AnimateFog(string id, float alpha, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValueInOut(Instance._fog_alpha, $"{nameof(AnimateFog)}_{id}", alpha, duration_in, duration_on, duration_out);

    public static Coroutine AnimateFogIn(string id, float alpha, float duration) =>
        Instance._AnimateMinValueIn(Instance._fog_alpha, $"{nameof(AnimateFog)}_{id}", alpha, duration);

    public static Coroutine AnimateFogOut(string id, float duration) =>
        Instance._AnimateMinValueOut(Instance._fog_alpha, $"{nameof(AnimateFog)}_{id}", 0, duration);

    // FOV //
    public static Coroutine AnimateFov(string id, float fov, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateValue(v => Instance._camera_fov = v, $"{nameof(AnimateFov)}_{id}", Instance._camera_fov_default, fov, duration_in, duration_on, duration_out);

    public static void SetCameraOffset(Vector3 offset) =>
        Instance._camera_offset = offset;

    public static void SetCameraOffsetForward(string id, float distance) =>
        Instance._camera_offset_forward.Set(id, distance);

    public static void RemoveCameraOffsetForward(string id) =>
        Instance._camera_offset_forward.Remove(id);

    // CAMERA SHAKE //
    public static Coroutine AnimateCameraShake(string id, float value, float duration_in, float duration_on, float duration_out) =>
        Instance._AnimateMinValueInOut(Instance._camera_shake_strength, $"{nameof(AnimateCameraShake)}_{id}", value, duration_in, duration_on, duration_out);

    /// <summary>
    /// Value range: 0 - 1
    /// </summary>
    public static Coroutine AnimateCameraShakeIn(string id, float value, float duration) =>
        Instance._AnimateMinValueIn(Instance._gaussian_amount, $"{nameof(AnimateCameraShake)}_{id}", value, duration);

    public static Coroutine AnimateCameraShakeOut(string id, float duration) =>
        Instance._AnimateMinValueOut(Instance._gaussian_amount, $"{nameof(AnimateCameraShake)}_{id}", 0, duration);

    // HEARTBEAT //
    public static Coroutine AnimateHeartbeatFrequency(string id, float frequency, float duration_in, float duration_on, float duration_out)
    {
        var cr_id = $"{nameof(AnimateHeartbeatFrequency)}_{id}";
        var cr = Coroutine.Start(Cr, cr_id);
        Instance.AddCoroutine(cr_id, cr);
        return cr;
        IEnumerator Cr()
        {
            yield return Instance._AnimateValue(f => Instance._heartbeat_frequency.Set(id, f), id, 1f, frequency, duration_in, duration_on, duration_out);
            Instance._heartbeat_frequency.Remove(id);
            Instance._coroutines.Remove(cr_id);
        }
    }

    public static void SetHeartbeatFrequency(string id, float strength) =>
        Instance._heartbeat_frequency.Set(id, strength);

    public static void RemoveHeartbeatFrequency(string id) =>
        Instance._heartbeat_frequency.Remove(id);

    public static void StartHeartbeat()
    {
        var id = $"{nameof(ScreenEffects)}_Heartbeat";
        Coroutine.Start(Cr, id);
        IEnumerator Cr()
        {
            while (true)
            {
                if (Instance._heartbeat_frequency.HasValues)
                {
                    var freq = Instance._heartbeat_frequency.Value;
                    var vol = Mathf.Lerp(0f, -15f, freq);
                    Beat();
                    SoundController.Instance.Play("sfx_heartbeat_A", new SoundOverride
                    {
                        Volume = vol
                    });
                    yield return new WaitForSeconds(0.3f * freq);
                    Beat();
                    SoundController.Instance.Play("sfx_heartbeat_B", new SoundOverride
                    {
                        Volume = vol
                    });
                    yield return new WaitForSeconds(0.8f * freq);
                }
                else
                {
                    yield return null;
                }
            }
        }

        void Beat()
        {
            AnimateFov(id, Instance._camera_fov - 3, 0, 0, 0.5f * Instance._heartbeat_frequency.Value);
        }
    }

    public static void StopHeartbeat()
    {
        var id = $"{nameof(ScreenEffects)}_Heartbeat";
        Coroutine.Stop(id);
    }
}
