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

    /// <summary>
    /// Sets AudioStreamPlayer's playing toggle, without requeing
    /// </summary>
    public static void SetPlaying(this AudioStreamPlayer3D asp, bool playing) =>
        _SetPlaying(asp, playing);

    /// <summary>
    /// Sets AudioStreamPlayer's playing toggle, without requeing
    /// </summary>
    public static void SetPlaying(this AudioStreamPlayer2D asp, bool playing) =>
        _SetPlaying(asp, playing);

    /// <summary>
    /// Sets AudioStreamPlayer's playing toggle, without requeing
    /// </summary>
    public static void SetPlaying(this AudioStreamPlayer asp, bool playing) =>
        _SetPlaying(asp, playing);

    private static void _SetPlaying(Node node, bool playing)
    {
        var id = "playing";
        var currently_playing = node.Get(id).AsBool();

        if (playing != currently_playing)
        {
            node.Set(id, playing);
        }
    }
}
