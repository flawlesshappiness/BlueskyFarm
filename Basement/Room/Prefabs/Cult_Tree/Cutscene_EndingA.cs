using Godot;
using System.Collections;

public partial class Cutscene_EndingA : Node3D
{
    [Export]
    public RootCore Core;

    [Export]
    public Marker3D PlayerPositionMarker;

    [Export]
    public Marker3D FrenLookMarker;

    [Export]
    public Marker3D CultistsLookMarker;

    [Export]
    public Marker3D CoreLookMarker;

    [Export]
    public Marker3D LookUpMarker;

    [Export]
    public DialogueCharacterInfo CharacterFrida;

    [Export]
    public DialogueCharacterInfo CharacterFrank;

    [Export]
    public DialogueCharacterInfo CharacterFren;

    [Export]
    public FrogCutscene Fren;

    [Export]
    public FrogCutscene Frida;

    [Export]
    public FrogCutscene Frank;

    [Export]
    public FrogCutscene Fritz;

    [Export]
    public SoundInfo BgmCutsceneTense;

    [Export]
    public SoundInfo BgmCutsceneDeath;

    private bool _active_dialogue;
    private AudioStreamPlayer asp_bgm;

    public override void _Ready()
    {
        base._Ready();
        DialogueController.Instance.OnDialogueTrigger += DialogueTrigger;
        DialogueController.Instance.OnDialogueEnd += DialogueEnd;
        DialogueController.Instance.OnCharacterChanged += CharacterChanged;

        Fren.Disable();
        Frank.Disable();
        Frida.Disable();
        Fritz.Disable();
    }

    public void StartEnding()
    {
        SetPlayerLocked(true);
        Cursor.Hide();
        asp_bgm = BgmCutsceneTense.Play();

        this.StartCoroutine(Cr, "Cutscene");
        IEnumerator Cr()
        {
            var cr_sword = Core.AnimateSwordIntoCore();
            var cr_move_player = AnimatePlayerToCutscenePosition();
            Player.Instance.StartLookingAt(CoreLookMarker, 0.05f);

            yield return cr_sword;
            yield return cr_move_player;

            StartDialogue("##ENDING_BAD_A_001##");
        }
    }

    private void SetPlayerLocked(bool locked)
    {
        var id = nameof(Cutscene_EndingA);

        Player.Instance.MovementLock.SetLock(id, true);
        Player.Instance.LookLock.SetLock(id, true);
        Player.Instance.InteractLock.SetLock(id, true);
        PauseView.Instance.ToggleLock.SetLock(id, true);
    }

    private void StartDialogue(string node)
    {
        _active_dialogue = true;
        DialogueController.Instance.SetNode(node);
    }

    private void DialogueTrigger(string trigger)
    {
        if (!_active_dialogue) return;

        switch (trigger)
        {
            case "look_at_cultists":
                LookAtCultists();
                break;

            case "look_at_fren":
                LookAtFren();
                break;

            case "character_frida":
                SetCharacterFrida();
                break;

            case "character_frank":
                SetCharacterFrank();
                break;

            case "character_fren":
                SetCharacterFren();
                break;

            case "enable_cultists":
                Frida.Enable();
                Frank.Enable();
                Fritz.Enable();
                break;

            case "enable_fren":
                Fren.Enable();
                break;

            default: break;
        }
    }

    private void DialogueEnd(DialogueNode node)
    {
        if (!_active_dialogue) return;

        if (node.id.Contains("ENDING_BAD_A"))
        {
            AnimateStabToCredits();
        }
        else if (node.id.Contains("ENDING_BAD_B"))
        {
            StartCredits();
        }
    }

    private void CharacterChanged(string name, DialogueCharacter character)
    {
        switch (name)
        {
            case "frida":
                character.SoundOrigin = CultistsLookMarker;
                break;

            case "frank":
                character.SoundOrigin = CultistsLookMarker;
                break;

            case "fren":
                character.SoundOrigin = FrenLookMarker;
                break;

            default: break;
        }
    }

    private void LookAtCultists()
    {
        Player.Instance.StartLookingAt(CultistsLookMarker, 0.05f);
    }

    private void LookAtFren()
    {
        Player.Instance.StartLookingAt(FrenLookMarker, 0.05f);
    }

    private void SetCharacterFrida()
    {
        DialogueController.Instance.SetCharacter(new DialogueCharacter
        {
            Info = CharacterFrida,
            SoundOrigin = CultistsLookMarker
        });
    }

    private void SetCharacterFrank()
    {
        DialogueController.Instance.SetCharacter(new DialogueCharacter
        {
            Info = CharacterFrank,
            SoundOrigin = CultistsLookMarker
        });
    }

    private void SetCharacterFren()
    {
        DialogueController.Instance.SetCharacter(new DialogueCharacter
        {
            Info = CharacterFren,
            SoundOrigin = FrenLookMarker
        });
    }

    private Coroutine AnimatePlayerToCutscenePosition()
    {
        return this.StartCoroutine(Cr, nameof(AnimatePlayerToCutscenePosition));
        IEnumerator Cr()
        {
            var start = Player.Instance.GlobalPosition;
            var end = PlayerPositionMarker.GlobalPosition;
            var curve = Curves.EaseOutQuad;
            yield return LerpEnumerator.Lerp01(1f, f =>
            {
                var t = curve.Evaluate(f);
                Player.Instance.GlobalPosition = start.Lerp(end, t);
            });
        }
    }

    private Coroutine AnimateStabToCredits()
    {
        return this.StartCoroutine(Cr, nameof(AnimateStabToCredits));
        IEnumerator Cr()
        {
            Fren.AnimationPlayer.Play("Armature|Sword_Stab");
            yield return new WaitForSeconds(0.4f);
            var bus = AudioBus.Get(SoundBus.SFX.ToString());
            bus.SetMuted(true);
            asp_bgm.Stop();
            BgmCutsceneDeath.Play();
            Player.Instance.StartLookingAt(LookUpMarker, 0.05f);
            ScreenEffects.AnimateCameraShake(nameof(Cutscene_EndingA), 0.25f, 0f, 0.25f, 0f);
            yield return AnimateRedFlash(0.25f);
            ScreenEffects.AnimateGaussianBlurIn(nameof(Cutscene_EndingA), 40, 3.0f);
            AnimateFadeOut(8f);
            yield return new WaitForSeconds(4f);

            StartDialogue("##ENDING_BAD_B_001##");
        }
    }

    private Coroutine AnimateRedFlash(float duration)
    {
        return this.StartCoroutine(Cr, nameof(AnimateRedFlash));
        IEnumerator Cr()
        {
            var overlay = GameView.Instance.CreateOverlay(Colors.Red.SetA(0.5f));
            var start = overlay.Color;
            var end = start.SetA(0);
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                overlay.Color = start.Lerp(end, f);
            });

            overlay.QueueFree();
        }
    }

    private Coroutine AnimateFadeOut(float duration)
    {
        return this.StartCoroutine(Cr, nameof(AnimateRedFlash));
        IEnumerator Cr()
        {
            var start = 0f;
            var end = 1f;
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                var a = Mathf.Lerp(start, end, f);
                GameView.Instance.SetBlackOverlayAlpha(a);
            });
        }
    }

    private void StartCredits()
    {
        SetPlayerLocked(false);

        Scene.Goto<CreditsScene>();
        GameView.Instance.SetBlackOverlayAlpha(0);
        ScreenEffects.AnimateGaussianBlurOut(nameof(Cutscene_EndingA), 0f);

        InventoryController.Instance.ResetInventory();

        var bus = AudioBus.Get(SoundBus.SFX.ToString());
        bus.SetMuted(false);
    }
}
