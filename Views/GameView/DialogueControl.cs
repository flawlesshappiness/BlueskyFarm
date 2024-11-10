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

    public override void _Ready()
    {
        base._Ready();

        DialogueController.Instance.OnDialogue += OnDialogue;
        DialogueController.Instance.OnDialogueStart += OnDialogueStart;
        DialogueController.Instance.OnDialogueEnd += OnDialogueEnd;
        Clear();

        var locales = TranslationServer.GetLoadedLocales();
        foreach (var locale in locales)
        {
            Debug.Log(locale);
        }
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
            var end = DialogueLabel.Text.Length;
            var speed = 20f;
            var value = start;
            while (value < end)
            {
                value += speed * GameTime.DeltaTime;
                DialogueLabel.VisibleCharacters = (int)value;
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

    private void AnimateBackgroundShow()
    {
        StartCoroutine(Cr, "background");
        IEnumerator Cr()
        {
            var duration = 1f;
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
