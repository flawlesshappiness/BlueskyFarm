using Godot;
using System.Collections;

public partial class DialogueControl : ControlScript
{
    [NodeName]
    public Control Background;

    [NodeName]
    public Label DialogueLabel;

    private bool _animating_text;
    private Coroutine _cr_animate_text;
    private float _time_next_voice;
    private Node _prev_text_sound;
    private Node _prev_voice_sound;

    public override void _Ready()
    {
        base._Ready();

        DialogueController.Instance.OnDialogue += OnDialogue;
        DialogueController.Instance.OnDialogueStart += OnDialogueStart;
        DialogueController.Instance.OnDialogueEnd += OnDialogueEnd;
        Clear();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (PlayerInput.Interact.Pressed)
        {
            if (_animating_text)
            {
                StopAnimatingText();
            }
            else
            {
                SetText("");
                DialogueController.Instance.NextNode();
            }
        }
    }

    private void Clear()
    {
        Background.Modulate = Background.Modulate.SetA(0);
        SetText("");
    }

    private void SetText(string text)
    {
        DialogueLabel.Text = text;
    }

    private void OnDialogue(DialogueNode node)
    {
        SetText(node.id);
        PlayVoiceSFX();
        AnimateText();
    }

    private void OnDialogueStart(DialogueNode node)
    {
        AnimateBackgroundShow();
    }

    private void OnDialogueEnd(DialogueNode node)
    {
        AnimateBackgroundHide();
        SetText("");
    }

    private void AnimateText()
    {
        DialogueLabel.VisibleCharacters = 0;
        _cr_animate_text = StartCoroutine(Cr, nameof(AnimateText));
        IEnumerator Cr()
        {
            _animating_text = true;

            var start = 0f;
            var end = Tr(DialogueLabel.Text).Length;
            var speed = 60f;
            var value = start;
            while (value < end)
            {
                value += speed * GameTime.DeltaTime;
                DialogueLabel.VisibleCharacters = (int)value;
                PlayTextSFX();
                yield return null;
            }

            DialogueLabel.VisibleCharacters = -1;
            _animating_text = false;
        }
    }

    private void StopAnimatingText()
    {
        Coroutine.Stop(_cr_animate_text);
        DialogueLabel.VisibleCharacters = -1;
        _animating_text = false;
    }

    private void PlayTextSFX()
    {
        if (GameTime.Time < _time_next_voice) return;
        _time_next_voice = GameTime.Time + 0.07f;

        if (IsInstanceValid(_prev_text_sound))
        {
            _prev_text_sound.Call("stop");
        }

        var character = DialogueController.Instance.CurrentCharacter;
        if (character != null && character.Info.TextSound != null)
        {
            _prev_text_sound = PlaySfx(character.Info.TextSound, null);
        }
    }

    private void PlayVoiceSFX()
    {
        if (IsInstanceValid(_prev_voice_sound))
        {
            _prev_voice_sound.Call("stop");
        }

        var character = DialogueController.Instance.CurrentCharacter;
        if (character != null && character.Info.VoiceDefault != null)
        {
            _prev_voice_sound = PlaySfx(character.Info.VoiceDefault, character.SoundOrigin);
        }
    }

    private Node PlaySfx(SoundInfo info, Node3D position)
    {
        if (position == null)
        {
            return SoundController.Instance.Play(info);
        }
        else
        {
            return SoundController.Instance.Play(info, position.GlobalPosition);
        }
    }

    private void AnimateBackgroundShow()
    {
        StartCoroutine(Cr, "background");
        IEnumerator Cr()
        {
            var duration = 0.5f;
            var start = Background.Modulate;
            var end = Background.Modulate.SetA(1);
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                Background.Modulate = start.Lerp(end, f);
            });
        }
    }

    private void AnimateBackgroundHide()
    {
        StartCoroutine(Cr, "background");
        IEnumerator Cr()
        {
            var duration = 1f;
            var start = Background.Modulate;
            var end = Background.Modulate.SetA(0);
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                Background.Modulate = start.Lerp(end, f);
            });
        }
    }
}
