using Godot;
using System.Collections;

public partial class TextView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/{nameof(TextView)}";

    [NodeName]
    public CanvasItem BottomText;

    [NodeName]
    public Label BottomTextLabel;

    public override void _Ready()
    {
        base._Ready();
        BottomText.Modulate = Colors.Transparent;
        Show();
    }

    public void DisplayText(string text)
    {
        if (text == BottomTextLabel.Text) return;

        var word_count = text.Split(' ').Length;
        var duration = Mathf.Max(3f, word_count);

        BottomTextLabel.Text = text;
        BottomText.Modulate = Colors.Transparent;

        SoundController.Instance.Play("sfx_text_display", new SoundSettings
        {
            Bus = SoundBus.SFX
        });

        Coroutine.Start(Cr, "DisplayText" + GetInstanceId());

        IEnumerator Cr()
        {
            var start = BottomText.Modulate;
            var end = Colors.White;
            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                BottomText.Modulate = start.Lerp(end, f);
            });

            yield return new WaitForSeconds(duration);

            start = BottomText.Modulate;
            end = Colors.Transparent;
            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                BottomText.Modulate = start.Lerp(end, f);
            });

            BottomTextLabel.Text = "";
        }
    }
}
