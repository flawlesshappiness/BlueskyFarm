using Godot;

public partial class GameView : View
{
    public static GameView Instance => View.Get<GameView>();

    [NodeName]
    public ColorRect OverlayTemplate;

    [NodeType]
    public Minimap Minimap;

    [NodeName]
    public InventoryBar InventoryBar;

    public override string Directory => $"{Paths.ViewDirectory}/{nameof(GameView)}";

    private ColorRect _overlay_black;

    public override void _Ready()
    {
        base._Ready();
        OverlayTemplate.Hide();
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
}
