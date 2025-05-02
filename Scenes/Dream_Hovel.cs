using Godot;
using System.Collections;

public partial class Dream_Hovel : DreamScene
{
    [Export]
    public SoundInfo SfxRumble;

    private AudioStreamPlayer asp_rumble;

    public override void StartScene()
    {
        base.StartScene();

        asp_rumble = SfxRumble.Play();

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            ScreenEffects.SetCameraShake(FxId, 0.4f);
            yield return new WaitForSeconds(8f);
            CompleteDream();
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        ScreenEffects.RemoveCameraShake(FxId);
        asp_rumble.Stop();
    }
}
