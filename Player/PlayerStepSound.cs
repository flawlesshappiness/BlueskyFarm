using Godot;

public partial class PlayerStepSound : NodeScript
{
    private FirstPersonStep Step => Player.Instance.Step;

    private bool is_first_step;
    private AudioStream _previous_step_sound;

    protected override void Initialize()
    {
        base.Initialize();
        Step.OnMoveStart += MoveStart;
        Step.OnMoveStep += MoveStep;
        Player.Instance.OnJump += Jump;
        Player.Instance.OnLand += Land;
    }

    private void MoveStart()
    {
        is_first_step = true;
    }

    private void MoveStep()
    {
        if (!is_first_step || Player.Instance.IsInWater)
        {
            var running = Player.Instance.IsRunning;
            var water = Player.Instance.IsInWater;
            var ground = Player.Instance.GetGround();
            var type = water ? SolidMaterialType.Water : ground?.Type ?? SolidMaterialType.Default;
            var info = SolidMaterialController.Instance.GetInfo(type);
            var pitch_base_water = running ? 0.8f : 1.0f;
            var pitch_base = water ? pitch_base_water : running ? 1.0f : 0.8f;
            var sfx = running ? info.RunSound : info.WalkSound;

            SoundController.Instance.Play(sfx, new SoundOverride
            {
                PitchRange = new Vector2(pitch_base - 0.05f, pitch_base)
            });
        }

        is_first_step = false;
    }

    private void Jump()
    {
    }

    private void Land()
    {
        Step.ResetStepTime();
    }
}
