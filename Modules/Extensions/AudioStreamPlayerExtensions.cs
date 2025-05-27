using Godot;
using System.Collections;

public static class AudioStreamPlayerExtensions
{
    public static Coroutine FadeOut(this AudioStreamPlayer3D asp, float duration) =>
        _Fade(asp, duration, -80f);

    public static Coroutine FadeOut(this AudioStreamPlayer2D asp, float duration) =>
        _Fade(asp, duration, -80f);

    public static Coroutine FadeOut(this AudioStreamPlayer asp, float duration) =>
        _Fade(asp, duration, -80f);

    public static Coroutine Fade(this AudioStreamPlayer3D asp, float duration, float volume) =>
        _Fade(asp, duration, volume);

    public static Coroutine Fade(this AudioStreamPlayer2D asp, float duration, float volume) =>
        _Fade(asp, duration, volume);

    public static Coroutine Fade(this AudioStreamPlayer asp, float duration, float volume) =>
        _Fade(asp, duration, volume);

    private static Coroutine _Fade(Node node, float duration, float to)
    {
        return Coroutine.Start(Cr, "fade_" + node.GetInstanceId(), node);
        IEnumerator Cr()
        {
            var start = AudioMath.DecibelToPercentage(node.Get("volume_db").AsSingle());
            var end = AudioMath.DecibelToPercentage(to);
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                var t = Mathf.Lerp(start, end, f);
                var db = AudioMath.PercentageToDecibel(t);
                node.Set("volume_db", db);
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
