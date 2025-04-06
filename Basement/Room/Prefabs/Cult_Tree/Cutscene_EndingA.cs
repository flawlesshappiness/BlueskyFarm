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
    public DialogueCharacterInfo CharacterFrida;

    [Export]
    public DialogueCharacterInfo CharacterFrank;

    [Export]
    public DialogueCharacterInfo CharacterFren;

    [Export]
    public Node3D Fren;

    [Export]
    public Node3D Frida;

    [Export]
    public Node3D Frank;

    [Export]
    public Node3D Fritz;

    private bool _active_dialogue;

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

    private void DialogueEnd()
    {
        if (!_active_dialogue) return;

        Scene.Goto<CreditsScene>();
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

            Debug.Log("Finished");
        }
    }
}
