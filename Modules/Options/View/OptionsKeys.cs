using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class OptionsKeys : NodeScript
{
    [Export]
    public string[] Actions;

    [NodeType]
    public OptionsKeyRebindControl TempKeyRebindControl;

    [NodeName]
    public Button ResetAllButton;

    public Dictionary<string, InputEvent> DefaultBindings { get; set; } = new();
    public List<Rebind> Rebinds { get; set; } = new();
    private Rebind _current_rebind;

    public Action OnRebindStart, OnRebindEnd;

    public bool IsRebinding => _current_rebind != null;

    public class Rebind
    {
        public InputEventData Data { get; set; }
        public OptionsKeyRebindControl Control { get; set; }
        public string Action { get; set; }
    }

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

    private void RebindStopped()
    {
        SetButtonsEnabled(true);
        OnRebindEnd?.Invoke();
    }

    public void PersistDefaultBindings()
    {
        DefaultBindings.Clear();
        var actions = InputMap.GetActions();
        foreach (var action in actions)
        {
            var e = InputMap.ActionGetEvents(action).FirstOrDefault();
            if (e == null) continue;
            DefaultBindings.Add(action, e);
        }
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

            var rebind = new Rebind
            {
                Control = control,
                Action = action,
                Data = data
            };

            Rebinds.Add(rebind);

            control.RebindButton.Pressed += () => PressRebind(rebind);
            control.ResetButton.Pressed += () => PressResetRebind(rebind);
        }
    }

    public void UpdateAllKeyStrings()
    {
        Rebinds.ForEach(x => UpdateKeyString(x));
    }

    private void UpdateKeyString(Rebind rebind)
    {
        var e = InputMap.ActionGetEvents(rebind.Action).First();
        var text = e.AsText().Replace("(Physical)", "").Trim();
        rebind.Control.RebindButton.Text = text;
    }

    public void UpdateDuplicateWarnings()
    {
        foreach (var current in Rebinds)
        {
            var current_input = InputMap.ActionGetEvents(current.Action).FirstOrDefault();
            if (current_input == null) continue;

            var found_duplicate = false;

            foreach (var other in Rebinds)
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

    private void PressRebind(Rebind rebind)
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

    private void PressResetRebind(Rebind rebind)
    {
        ResetRebind(rebind);
        UpdateDuplicateWarnings();
        Data.Game.Save();
    }

    private void ResetAllRebinds()
    {
        foreach (var rebind in Rebinds)
        {
            ResetRebind(rebind);
        }
    }

    private void ResetRebind(Rebind rebind)
    {
        var binding = DefaultBindings[rebind.Action];
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
        RebindStopped();
    }

    private void SetButtonsEnabled(bool enabled)
    {
        foreach (var rebind in Rebinds)
        {
            rebind.Control.RebindButton.Disabled = !enabled;
            rebind.Control.ResetButton.Disabled = !enabled;
        }

        ResetAllButton.Disabled = !enabled;
    }
}
