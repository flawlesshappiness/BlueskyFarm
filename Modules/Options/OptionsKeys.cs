using Godot;
using System;
using System.Linq;

public partial class OptionsKeys : NodeScript
{
    [Export]
    public string[] Actions;

    [NodeType]
    public OptionsKeyRebindControl TempKeyRebindControl;

    [NodeName]
    public Button ResetAllButton;

    public bool IsRebinding => _current_rebind != null;

    public event Action OnRebindStart, OnRebindEnd;

    private OptionsKeyRebind _current_rebind;

    public override void _Ready()
    {
        base._Ready();
        CreateKeys();

        ResetAllButton.Pressed += PressResetAllRebinds;
    }

    private void RebindStarted()
    {
        SetButtonsEnabled(false);
        OnRebindStart?.Invoke();
    }

    private void RebindEnded()
    {
        SetButtonsEnabled(true);
        OnRebindEnd?.Invoke();
    }

    private void CreateKeys()
    {
        TempKeyRebindControl.Visible = false;

        var parent = TempKeyRebindControl.GetParent();
        foreach (var action in Actions)
        {
            var control = TempKeyRebindControl.Duplicate() as OptionsKeyRebindControl;
            control.SetParent(parent);
            control.Visible = true;
            control.RebindLabel.Text = action;
            control.SetWaitingForInput(false);

            var data_key = Data.Options.KeyOverrides.FirstOrDefault(x => x.Action == action) as InputEventData;
            var data_mouse = Data.Options.MouseButtonOverrides.FirstOrDefault(x => x.Action == action) as InputEventData;
            var data = data_key ?? data_mouse;

            var rebind = new OptionsKeyRebind
            {
                Control = control,
                Action = action,
                Data = data
            };

            OptionsController.Rebinds.Add(rebind);

            control.RebindButton.Pressed += () => PressRebind(rebind);
            control.ResetButton.Pressed += () => PressResetRebind(rebind);
        }
    }

    public void UpdateAllKeyStrings()
    {
        OptionsController.Rebinds.ForEach(x => UpdateKeyString(x));
    }

    private void UpdateKeyString(OptionsKeyRebind rebind)
    {
        var e = InputMap.ActionGetEvents(rebind.Action).First();
        var text = e.AsText().Replace("(Physical)", "").Trim();
        rebind.Control.RebindButton.Text = text;
    }

    public void UpdateDuplicateWarnings()
    {
        foreach (var current in OptionsController.Rebinds)
        {
            var current_input = InputMap.ActionGetEvents(current.Action).FirstOrDefault();
            if (current_input == null) continue;

            var found_duplicate = false;

            foreach (var other in OptionsController.Rebinds)
            {
                if (current == other) continue;

                var other_input = InputMap.ActionGetEvents(other.Action).FirstOrDefault();
                if (other_input == null) continue;

                var same = current_input.AsText() == other_input.AsText();
                found_duplicate = same || found_duplicate;
                current.Control.DuplicateWarningLabel.Visible = found_duplicate;
            }
        }
    }

    private void PressRebind(OptionsKeyRebind rebind)
    {
        if (_current_rebind != null) return;

        _current_rebind = rebind;
        _current_rebind.Control.SetWaitingForInput(true);
        RebindStarted();
    }

    private void PressResetAllRebinds()
    {
        ResetAllRebinds();
        UpdateDuplicateWarnings();
        Data.Game.Save();
    }

    private void PressResetRebind(OptionsKeyRebind rebind)
    {
        ResetRebind(rebind);
        UpdateDuplicateWarnings();
        Data.Game.Save();
    }

    private void ResetAllRebinds()
    {
        foreach (var rebind in OptionsController.Rebinds)
        {
            ResetRebind(rebind);
        }
    }

    private void ResetRebind(OptionsKeyRebind rebind)
    {
        var binding = OptionsController.DefaultBindings[rebind.Action];
        InputMap.ActionEraseEvents(rebind.Action);
        InputMap.ActionAddEvent(rebind.Action, binding);
        rebind.Data = null;

        UpdateKeyString(rebind);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (_current_rebind == null) return;

        if (IsCancelEvent(@event))
        {
            StopRebinding();
        }
        else if (@event is InputEventKey)
        {
            OverrideKey(@event as InputEventKey);
            StopRebinding();
        }
        else if (@event is InputEventMouseButton)
        {
            OverrideMouseButton(@event as InputEventMouseButton);
            StopRebinding();
        }
    }

    private void OverrideKey(InputEventKey e)
    {
        var data = InputEventKeyData.Create(_current_rebind.Action, e);
        _current_rebind.Data = data;
        OptionsController.Instance.UpdateKeyOverride(data);
    }

    private void OverrideMouseButton(InputEventMouseButton e)
    {
        var data = InputEventMouseButtonData.Create(_current_rebind.Action, e);
        _current_rebind.Data = data;
        OptionsController.Instance.UpdateMouseButtonOverride(data);
    }

    private bool IsCancelEvent(InputEvent e)
    {
        var key_event = e as InputEventKey;
        if (key_event == null) return false;
        return key_event.KeyLabel == Key.Escape;
    }

    private void StopRebinding()
    {
        _current_rebind.Control.SetWaitingForInput(false);
        _current_rebind = null;
        UpdateAllKeyStrings();
        UpdateDuplicateWarnings();
        RebindEnded();
    }

    private void SetButtonsEnabled(bool enabled)
    {
        foreach (var rebind in OptionsController.Rebinds)
        {
            rebind.Control.RebindButton.Disabled = !enabled;
            rebind.Control.ResetButton.Disabled = !enabled;
        }

        ResetAllButton.Disabled = !enabled;
    }
}
