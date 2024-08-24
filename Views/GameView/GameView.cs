using Godot;
using System.Collections;

public partial class GameView : View
{
    [NodeName]
    public ColorRect ColorRectOverlay;

    [NodeType]
    public Minimap Minimap;

    [NodeName]
    public DynamicUI DynamicUI;

    [NodeName]
    public Label BottomTextLabel;

    public override string Directory => $"{Paths.ViewDirectory}/{nameof(GameView)}";

    public override void _Ready()
    {
        base._Ready();
        BottomTextLabel.Modulate = Colors.Transparent;
        SetOverlayAlpha(0);
        Show();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (PlayerInput.ShowUI.Pressed)
        {
            DynamicUI.AnimateShow(true);
        }
    }

    public void SetOverlayAlpha(float alpha)
    {
        var color = ColorRectOverlay.Color;
        var new_color = new Color(color.R, color.G, color.B, alpha);
        ColorRectOverlay.Color = new_color;
    }

    public void DisplayText(string text)
    {
        if (text == BottomTextLabel.Text) return;

        var word_count = text.Split(' ').Length;
        var duration = Mathf.Max(3f, word_count);

        BottomTextLabel.Text = text;
        BottomTextLabel.Modulate = Colors.Transparent;

        SoundController.Instance.Play("sfx_text_display", new SoundSettings
        {
            Bus = SoundBus.SFX
        });

        Coroutine.Start(Cr, "DisplayText" + GetInstanceId());

        IEnumerator Cr()
        {
            var start = BottomTextLabel.Modulate;
            var end = Colors.White;
            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                BottomTextLabel.Modulate = start.Lerp(end, f);
            });

            yield return new WaitForSeconds(duration);

            start = BottomTextLabel.Modulate;
            end = Colors.Transparent;
            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                BottomTextLabel.Modulate = start.Lerp(end, f);
            });

            BottomTextLabel.Text = "";
        }
    }
}
