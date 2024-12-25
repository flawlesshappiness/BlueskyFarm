using Godot;
using System.Collections;
using System.Collections.Generic;

public partial class GameView : View
{
    public static GameView Instance => View.Get<GameView>();

    [NodeName]
    public ColorRect OverlayTemplate;

    [NodeName]
    public Label TextTemplate;

    [NodeType]
    public Minimap Minimap;

    [NodeName]
    public InventoryBar InventoryBar;

    public override string Directory => $"{Paths.ViewDirectory}/{nameof(GameView)}";

    private ColorRect _overlay_black;
    private Dictionary<string, Label> _create_text_labels = new();

    public override void _Ready()
    {
        base._Ready();
        OverlayTemplate.Hide();
        TextTemplate.Hide();
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

    public void ShowAllDynamicUI()
    {
        InventoryBar.AnimateShow();
    }

    public void HideAllDynamicUI()
    {
        InventoryBar.AnimateHide();
    }

    public Label CreateText(CreateTextSettings settings)
    {
        if (_create_text_labels.TryGetValue(settings.Id, out var label))
        {
            if (IsInstanceValid(label))
            {
                label.QueueFree();
            }
            _create_text_labels.Remove(settings.Id);
        }

        label = TextTemplate.Duplicate() as Label;
        label.SetParent(TextTemplate.GetParent());
        label.Text = settings.Text;
        _create_text_labels.Add(settings.Id, label);

        var time_start = GameTime.Time;
        var time_end = time_start + settings.Duration;
        var camera = ScreenEffects.View.Camera;

        IEnumerator Cr()
        {
            while (GameTime.Time < time_end || settings.Duration == -1)
            {
                var world_position = settings.Target.GlobalPosition + settings.Offset;
                var viewport_position = camera.UnprojectPosition(world_position);
                var size = label.Size;
                var label_position = viewport_position - size * 0.5f;
                var offset = GetOffsetPosition();
                label.Position = label_position + offset;
                label.Visible = !camera.IsPositionBehind(world_position);
                yield return null;
            }

            label.QueueFree();
        }

        Vector2 GetOffsetPosition()
        {
            return OffsetShake();
        }

        float _shake_next = 0f;
        Vector2 _shake_prev = Vector2.Zero;
        Vector2 OffsetShake()
        {
            var end = time_start + settings.Shake_Duration;

            if (GameTime.Time >= end) return Vector2.Zero;
            if (GameTime.Time < _shake_next) return _shake_prev;

            var rng = new RandomNumberGenerator();
            var x = rng.RandfRange(-1, 1);
            var y = rng.RandfRange(-1, 1);
            _shake_prev = new Vector2(x, y).Normalized() * settings.Shake_Strength;
            _shake_next = GameTime.Time + settings.Shake_Frequency;
            settings.Shake_Strength *= settings.Shake_Dampening;
            return _shake_prev;
        }

        Coroutine.Start(Cr, settings.Id, label);
        return label;
    }
}

public class CreateTextSettings
{
    public string Id { get; set; }
    public string Text { get; set; }
    public Vector3 Offset { get; set; }
    public Node3D Target { get; set; }
    public float Duration { get; set; } = -1;
    public float Shake_Strength { get; set; }
    public float Shake_Frequency { get; set; }
    public float Shake_Duration { get; set; }
    public float Shake_Dampening { get; set; }
}