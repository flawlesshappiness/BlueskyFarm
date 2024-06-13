public partial class PlayerStepSound : NodeScript
{
    [NodeName(nameof(SfxStep))]
    public Sound SfxStep;

    [NodeName(nameof(SfxLand))]
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
            SfxStep.Play();
        }

        is_first_step = false;
    }

    private void Jump()
    {
        SfxStep.Play();
    }

    private void Land()
    {
        SfxLand.Play();
        Step.ResetStepTime();
    }
}
