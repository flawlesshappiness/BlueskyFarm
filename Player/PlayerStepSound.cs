public partial class PlayerStepSound : NodeScript
{
    [NodeName]
    public Sound SfxWalk;

    [NodeName]
    public Sound SfxRun;

    [NodeName]
    public Sound SfxLand;

    private FirstPersonController Player => FirstPersonController.Instance;
    private FirstPersonStep Step => Player.Step;

    private bool is_first_step;

    protected override void Initialize()
    {
        base.Initialize();
        Step.OnMoveStart += MoveStart;
        Step.OnMoveStep += MoveStep;
        Player.OnJump += Jump;
        Player.OnLand += Land;
    }

    private void MoveStart()
    {
        is_first_step = true;
    }

    private void MoveStep()
    {
        if (!is_first_step)
        {
            var sfx = Player.IsRunning ? SfxRun : SfxWalk;
            sfx.Play();
        }

        is_first_step = false;
    }

    private void Jump()
    {
        SfxRun.Play();
    }

    private void Land()
    {
        Step.ResetStepTime();
        SfxLand.Play();
    }
}
