using Godot;
using System.Collections;

public partial class GameView : View
{
    public static GameView Instance => View.Get<GameView>();

    [NodeName]
    public ColorRect OverlayTemplate;

    [NodeType]
    public Minimap Minimap;

    [NodeName]
    public InventoryBar InventoryBar;

    [NodeName]
    public Label BottomTextLabel;

    public override string Directory => $"{Paths.ViewDirectory}/{nameof(GameView)}";

    private ColorRect _overlay_black;

    public override void _Ready()
    {
        base._Ready();
        OverlayTemplate.Hide();
        BottomTextLabel.Modulate = Colors.Transparent;
        _overlay_black = CreateOverlay(Colors.Black.SetA(0));
        Show();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (PlayerInput.ShowUI.Pressed)
        {
            ShowAllDynamicUI();
        }
    }

    public ColorRect CreateOverlay(Color color)
    {
        var overlay = OverlayTemplate.Duplicate() as ColorRect;
        overlay.SetParent(OverlayTemplate.GetParent());
        overlay.Color = color;
        overlay.Show();
        return overlay;
    }

    public void SetBlackOverlayAlpha(float alpha)
    {
        var color = _overlay_black.Color;
        var new_color = new Color(color.R, color.G, color.B, alpha);
        _overlay_black.Color = new_color;
    }

    public void DisplayText(string text)
    {
        if (text == BottomTextLabel.Text) return;

        var word_count = text.Split(' ').Length;
        var duration = Mathf.Max(3f, word_count);

        BottomTextLabel.Text = text;
        BottomTextLabel.Modulate = Colors.Transparent;

        SoundController.Instance.Play("sfx_text_display", new SoundOverride
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

    public void ShowAllDynamicUI()
    {
        InventoryBar.AnimateShow();
    }

    public void HideAllDynamicUI()
    {
        InventoryBar.AnimateHide();
    }
}
