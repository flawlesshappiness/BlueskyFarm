using Godot;
using System.Collections;
using System.Collections.Generic;

public partial class Dream_Office : DreamScene
{
    private List<Key> _keys = new List<Key> { Key.W, Key.A, Key.K, Key.E, Key.U, Key.P };
    private int _idx_key;
    private bool _ready_for_input;

    public override void _Ready()
    {
        base._Ready();

        _idx_key = 0;
        _ready_for_input = false;

        EnableInput(3f);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventKey input && input.IsReleased())
        {
            KeyPressed(input.Keycode);
        }
    }

    private void KeyPressed(Key key)
    {
        if (!_ready_for_input) return;

        var expected_key = GetExpectedKey();

        if (key == expected_key)
        {
            _ready_for_input = false;
            HideText();

            if (HasMoreKeys())
            {
                _idx_key++;
                EnableInput(0.25f);
            }
            else
            {
                CompleteDream();
            }
        }
    }

    private void EnableInput(float delay)
    {
        this.StartCoroutine(Cr, nameof(EnableInput));
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);
            _ready_for_input = true;

            ShowText();
        }
    }

    private void ShowText()
    {
        Cursor.Show(new CursorSettings
        {
            Text = "Type " + GetExpectedKey().ToString(),
            Position = Camera.GlobalPosition - Camera.Basis.Z
        });
    }

    private void HideText()
    {
        Cursor.Hide();
    }

    private Key GetExpectedKey()
    {
        return _keys[Mathf.Clamp(_idx_key, 0, _keys.Count - 1)];
    }

    private bool HasMoreKeys()
    {
        return _idx_key < _keys.Count - 1;
    }
}
