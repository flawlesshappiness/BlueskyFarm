using Godot;
using Godot.Collections;
using System.Collections;
using System.Collections.Generic;

public partial class Cutscene_EndingB : Node3D
{
    [Export]
    public EnvironmentInfo Environment;

    [Export]
    public DialogueCharacterInfo DialogueCharacterInfo;

    [Export]
    public CultHoleFire HoleFire;

    [Export]
    public AudioStreamPlayer SfxGlassBreak;

    [Export]
    public AudioStreamPlayer SfxBackgroundFire;

    [Export]
    public Array<Marker3D> CameraMarkers;

    private int idx_sequence;

    private List<string> dialogue_nodes = new() { "##CUTSCENE_B_001##", "##CUTSCENE_B_002##", "##CUTSCENE_B_003##" };

    public override void _Ready()
    {
        base._Ready();
        DialogueController.Instance.OnDialogueEnd += DialogueEnd;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        DialogueController.Instance.OnDialogueEnd -= DialogueEnd;
    }

    public void StartEnding()
    {
        SetPlayerLocked(true);
        Cursor.Hide();
        GameView.Instance.SetBlackOverlayAlpha(1);

        HoleFire.Start();

        EnvironmentController.Instance.SetEnvironment(Environment);

        idx_sequence = 0;

        AnimateFirstSequencePosition();
    }

    private void DialogueEnd(DialogueNode node)
    {
        if (node.id.Contains("CUTSCENE_B_003"))
        {
            StartCredits();
        }
        else if (node.id.Contains("CUTSCENE_B"))
        {
            idx_sequence++;
            AnimateNextSequencePosition();
        }
    }

    private Coroutine AnimateFirstSequencePosition()
    {
        return this.StartCoroutine(Cr, "sequence");
        IEnumerator Cr()
        {
            SfxGlassBreak.Play();
            yield return new WaitForSeconds(2f);
            AmbienceController.Instance.StopAmbience();
            var db_end = SfxBackgroundFire.VolumeDb;
            SfxBackgroundFire.VolumeDb = -80f;
            SfxBackgroundFire.Play();
            yield return SfxBackgroundFire.FadeIn(2f, SfxBackgroundFire.VolumeDb);
            AnimateNextSequencePosition();
        }
    }
    private Coroutine AnimateNextSequencePosition()
    {
        return this.StartCoroutine(Cr, "sequence");
        IEnumerator Cr()
        {
            var camera_marker = GetCameraMarker(idx_sequence);
            var dialogue_node = GetDialogueNode(idx_sequence);

            ScreenEffects.View.SetCameraTarget(camera_marker);
            SoundController.Instance.Play("sfx_horror_boom");

            yield return LerpEnumerator.Lerp01(3f, f =>
            {
                var a = Mathf.Lerp(0f, 1f, f);
                GameView.Instance.SetBlackOverlayAlpha(a);
            });

            yield return new WaitForSeconds(1f);

            DialogueController.Instance.SetCharacter(new DialogueCharacter { Info = DialogueCharacterInfo });
            DialogueController.Instance.SetNode(dialogue_node);
        }
    }

    private string GetDialogueNode(int idx)
    {
        return dialogue_nodes.GetClamped(idx);
    }

    private Marker3D GetCameraMarker(int idx)
    {
        return CameraMarkers.GetClamped(idx);
    }

    private void SetPlayerLocked(bool locked)
    {
        var id = nameof(Cutscene_EndingB);

        Player.Instance.MovementLock.SetLock(id, locked);
        Player.Instance.LookLock.SetLock(id, locked);
        Player.Instance.InteractLock.SetLock(id, locked);
        PauseView.Instance.ToggleLock.SetLock(id, locked);
        InventoryController.Instance.InventoryLock.SetLock(id, locked);
    }

    private void StartCredits()
    {
        SetPlayerLocked(false);

        Scene.Goto<CreditsScene>();
        GameView.Instance.SetBlackOverlayAlpha(0);
        ScreenEffects.AnimateGaussianBlurOut(nameof(Cutscene_EndingB), 0f);

        InventoryController.Instance.ResetInventory();

        var bus = AudioBus.Get(SoundBus.SFX.ToString());
        bus.SetMuted(false);
    }
}
