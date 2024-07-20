using Godot;
using System.Collections;

public static class ScreenEffects
{
    private static ScreenEffectsView _view;
    public static ScreenEffectsView Instance => _view ?? (_view = View.Get<ScreenEffectsView>());

    private enum BlurType
    {
        GAUSSIAN = 0,
        RADIAL = 1
    }

    private static Coroutine _cr_blur;
    private static Coroutine _cr_radial;
    private static Coroutine _cr_distort;

    public static void Reset()
    {
        Coroutine.Stop(_cr_blur);
        Coroutine.Stop(_cr_radial);
        Coroutine.Stop(_cr_distort);

        Instance.Blur_Type = -1;
        Instance.Blur_Amount = 0;
        Instance.Radial_Strength = 0;
        Instance.Distort_Strength = 0;
    }

    private static void SetBlurType(BlurType blur_type)
    {
        Instance.Blur_Type = (int)blur_type;
    }

    public static Coroutine AnimateBlur(float strength, float duration_in, float duration_on, float duration_out)
    {
        Coroutine.Stop(_cr_blur);
        _cr_blur = Coroutine.Start(Cr);
        return _cr_blur;

        IEnumerator Cr()
        {
            SetBlurType(BlurType.GAUSSIAN);
            var start = Instance.Blur_Amount;
            var end = strength;

            yield return LerpEnumerator.Lerp01(duration_in, f =>
            {
                Instance.Blur_Amount = Mathf.Lerp(start, end, f);
            });

            yield return new WaitForSeconds(duration_on);

            start = Instance.Blur_Amount;
            end = 0f;

            yield return LerpEnumerator.Lerp01(duration_out, f =>
            {
                Instance.Blur_Amount = Mathf.Lerp(start, end, f);
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
            Instance.Distort_Speed = speed;
            Instance.Distort_Displacement = displacement;

            var start = Instance.Distort_Strength;
            var end = strength;

            yield return LerpEnumerator.Lerp01(duration_in, f =>
            {
                Instance.Distort_Strength = Mathf.Lerp(start, end, f);
            });

            yield return new WaitForSeconds(duration_on);

            start = Instance.Distort_Strength;
            end = 0f;

            yield return LerpEnumerator.Lerp01(duration_out, f =>
            {
                Instance.Distort_Strength = Mathf.Lerp(start, end, f);
            });
        }
    }
}
