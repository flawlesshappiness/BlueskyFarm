using Godot;

public partial class OptionsKeyRebindControl : ControlScript
{
    [NodeName]
    public Label RebindLabel;

    [NodeName]
    public Label WaitingForInputLabel;

    [NodeName]
    public Label DuplicateWarningLabel;

    [NodeName]
    public Button RebindButton;

    [NodeName]
    public Button ResetButton;

    public void SetWaitingForInput(bool waiting)
    {
        RebindButton.Visible = !waiting;
        WaitingForInputLabel.Visible = waiting;
        ResetButton.Disabled = waiting;
    }
}
