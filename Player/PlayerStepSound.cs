using Godot;
using System.Linq;

public partial class PlayerStepSound : NodeScript
{
    private FirstPersonController Player => FirstPersonController.Instance;
    private FirstPersonStep Step => Player.Step;

    private bool is_first_step;
    private AudioStream _previous_step_sound;

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
            var ground = Player.GetGround();
            var info = SolidMaterialController.Instance.GetInfo(ground?.Type ?? SolidMaterialType.Default);
            var volume = Player.IsRunning ? info.StepBaseVolume + 3 : info.StepBaseVolume;
            var pitch_base = Player.IsRunning ? 1.0f : 0.8f;
            var streams = Player.IsRunning ? info.WalkSounds : info.RunSounds;
            var valid_streams = streams.Where(x => _previous_step_sound == null || x != _previous_step_sound);
            var sfx = valid_streams.ToList().Random();
            _previous_step_sound = sfx;

            SoundController.Instance.Play(sfx, new SoundSettings
            {
                Bus = SoundBus.SFX,
                PitchMin = pitch_base - 0.1f,
                PitchMax = pitch_base,
                Volume = volume
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
