using Godot;
using System.Collections;

public static class ScreenEffects
{
    private static ScreenEffectsView _view;
    public static ScreenEffectsView Instance => _view ?? (_view = View.Get<ScreenEffectsView>());

    private static Coroutine _cr_blur;
    private static Coroutine _cr_radial;
    private static Coroutine _cr_distort;
    private static Coroutine _cr_fog;

    public static void Reset()
    {
        Coroutine.Stop(_cr_blur);
        Coroutine.Stop(_cr_radial);
        Coroutine.Stop(_cr_distort);
        Coroutine.Stop(_cr_fog);

        Instance.Reset();
    }

    public static Coroutine AnimateBlur(float strength, float duration_in, float duration_on, float duration_out)
    {
        Coroutine.Stop(_cr_blur);
        _cr_blur = Coroutine.Start(Cr);
        return _cr_blur;

        IEnumerator Cr()
        {
            var start = Instance.GaussianBlurAmount;
            var end = strength;

            yield return LerpEnumerator.Lerp01(duration_in, f =>
            {
                Instance.GaussianBlurAmount = Mathf.Lerp(start, end, f);
            });

            yield return new WaitForSeconds(duration_on);

            start = Instance.GaussianBlurAmount;
            end = 0f;

            yield return LerpEnumerator.Lerp01(duration_out, f =>
            {
                Instance.GaussianBlurAmount = Mathf.Lerp(start, end, f);
            });
        }
    }

    public static Coroutine AnimateDistort(float strength, float speed, Vector2 displacement, float duration_in, float duration_on, float duration_out)
    {
        Coroutine.Stop(_cr_distort);
        _cr_distort = Coroutine.Start(Cr);
        return _cr_distort;

        IEnumerator Cr()
        {
            Instance.DistortSpeed = speed;
            Instance.DistortDisplacement = displacement;

            var start = Instance.DistortStrength;
            var end = strength;

            yield return LerpEnumerator.Lerp01(duration_in, f =>
            {
                Instance.DistortStrength = Mathf.Lerp(start, end, f);
            });

            yield return new WaitForSeconds(duration_on);

            start = Instance.DistortStrength;
            end = 0f;

            yield return LerpEnumerator.Lerp01(duration_out, f =>
            {
                Instance.DistortStrength = Mathf.Lerp(start, end, f);
            });
        }
    }

    public static Coroutine AnimateFog(float alpha, Color color, Vector2 direction, float duration_in, float duration_on, float duration_out)
    {
        Coroutine.Stop(_cr_fog);
        _cr_fog = Coroutine.Start(Cr);
        return _cr_fog;

        IEnumerator Cr()
        {
            Instance.Fog_Color = new Color(color.R, color.G, color.B, Instance.Fog_Alpha);
            Instance.Fog_Direction = direction;

            var start = Instance.Fog_Alpha;
            var end = alpha * color.A;

            yield return LerpEnumerator.Lerp01(duration_in, f =>
            {
                Instance.Fog_Alpha = Mathf.Lerp(start, end, f);
            });

            yield return new WaitForSeconds(duration_on);

            start = Instance.Fog_Alpha;
            end = 0f;

            yield return LerpEnumerator.Lerp01(duration_out, f =>
            {
                Instance.Fog_Alpha = Mathf.Lerp(start, end, f);
            });
        }
    }
}
