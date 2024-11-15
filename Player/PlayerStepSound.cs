using Godot;

public partial class PlayerStepSound : NodeScript
{
    protected override void Initialize()
    {
        base.Initialize();
        Player.Instance.Step.OnMoveStep += MoveStep;
        Player.Instance.OnMoveStart += MoveStart;
        Player.Instance.OnLand += Land;
    }

    private void MoveStart()
    {
        if (Player.Instance.IsInWater)
        {
            MoveStep();
        }
    }

    private void MoveStep()
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

    private void Land()
    {
        Player.Instance.Step.ResetStepTime();
    }
}
