using Godot;

public partial class OptionsKeyRebindControl : ControlScript
{
    [NodeName(nameof(RebindLabel))]
    public Label RebindLabel;

    [NodeName(nameof(WaitingForInputLabel))]
    public Label WaitingForInputLabel;

    [NodeName(nameof(RebindButton))]
    public Button RebindButton;

    [NodeName(nameof(ResetButton))]
    public Button ResetButton;

    public void SetWaitingForInput(bool waiting)
    {
        RebindButton.Visible = !waiting;
        WaitingForInputLabel.Visible = waiting;
        ResetButton.Disabled = waiting;
    }
}
