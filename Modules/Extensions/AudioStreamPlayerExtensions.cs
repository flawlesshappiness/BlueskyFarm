using Godot;
using System.Collections;

public static class AudioStreamPlayerExtensions
{
    public static Coroutine FadeOut(this AudioStreamPlayer3D asp, float duration) =>
        _FadeOut(asp, duration);

    public static Coroutine FadeOut(this AudioStreamPlayer2D asp, float duration) =>
        _FadeOut(asp, duration);

    public static Coroutine FadeOut(this AudioStreamPlayer asp, float duration) =>
        _FadeOut(asp, duration);

    private static Coroutine _FadeOut(Node node, float duration)
    {
        return Coroutine.Start(Cr, "fadeout_" + node.GetInstanceId(), node);
        IEnumerator Cr()
        {
            var start = node.Get("volume_db").AsSingle();
            var end = -80f;
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                var v = Mathf.Lerp(start, end, f);
                node.Set("volume_db", v);
            });
        }
    }
}
