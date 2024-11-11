using Godot;
using System.Collections;

public partial class DialogueControl : ControlScript
{
    [NodeName]
    public Control Background;

    [NodeName]
    public Label DialogueLabel;

    [NodeName]
    public AudioStreamPlayer SfxVoice;

    private bool _animating_text;
    private Coroutine _cr_animate_text;
    private float _time_next_voice;

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
        SetText(node.name);
        AnimateText();
    }

    private void OnDialogueStart()
    {
        AnimateBackgroundShow();
    }

    private void OnDialogueEnd()
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
            var speed = 30f;
            var value = start;
            while (value < end)
            {
                value += speed * GameTime.DeltaTime;
                DialogueLabel.VisibleCharacters = (int)value;
                PlayVoiceSFX();
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

    private void PlayVoiceSFX()
    {
        if (GameTime.Time < _time_next_voice) return;
        _time_next_voice = GameTime.Time + 0.07f;

        var rng = new RandomNumberGenerator();
        SfxVoice.PitchScale = rng.RandfRange(0.95f, 1f);
        SfxVoice.Play();
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
