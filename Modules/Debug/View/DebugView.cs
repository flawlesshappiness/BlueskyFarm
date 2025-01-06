using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DebugView : View
{
    public override string Directory => $"{Paths.Modules}/Debug/View";

    [NodeName]
    public Control Main;

    [NodeName]
    public Control Content;

    [NodeName]
    public DebugInputPopup InputPopup;

    [NodeName]
    public Button ButtonPrefab;

    [NodeName]
    public Label CategoryPrefab;

    [NodeName]
    public DebugContentSearch ContentSearch;

    [NodeName]
    public DebugContentList ContentList;

    [NodeName]
    public AudioStreamPlayer SfxClick;

    [NodeName]
    public AudioStreamPlayer SfxHover;

    [NodeName]
    public AudioStreamPlayer SfxOpen;

    private Dictionary<string, Label> _categories = new();
    private List<Button> _buttons = new();
    private Action<Dictionary<string, string>> _onInputPopupSuccess;

    public override void _Ready()
    {
        base._Ready();
        Visible = true;
        ButtonPrefab.Visible = false;
        CategoryPrefab.Visible = false;
        HideContent();
        SetVisible(false);

        Debug.RegisterDebugActions();

        InputPopup.OnSuccess += InputPopupSuccess;
        InputPopup.OnCancel += InputPopupCancel;
    }

    private void InputPopupCancel()
    {
        InputPopup.Hide();
        Main.Show();
    }

    private void InputPopupSuccess(Dictionary<string, string> results)
    {
        _onInputPopupSuccess?.Invoke(results);
        InputPopup.Hide();
        Main.Show();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (Input.IsActionJustPressed("ui_text_indent"))
            {
                ToggleVisible();
                SfxOpen.Play();
            }
        }
    }

    public void SetVisible(bool visible)
    {
        Main.Visible = visible;
        InputPopup.Visible = false;

        var parent = GetParent();
        var lock_name = nameof(DebugView);
        if (visible)
        {
            MouseVisibility.Instance.Lock.AddLock(lock_name);
            Scene.PauseLock.AddLock(lock_name);

            var idx_max = parent.GetChildCount() - 1;
            parent.MoveChild(this, idx_max);

            CreateActionButtons();
        }
        else
        {
            MouseVisibility.Instance.Lock.RemoveLock(lock_name);
            Scene.PauseLock.RemoveLock(lock_name);

            parent.MoveChild(this, 0);

            HideContent();

            Clear();
        }
    }

    public void Close() => SetVisible(false);

    public void HideContent()
    {
        Content.Visible = false;
        ContentSearch.Visible = false;
        ContentList.Visible = false;
    }

    public void SetContent_Search()
    {
        HideContent();
        Content.Show();
        ContentSearch.Show();
        ContentSearch.ClearItems();
    }

    public void SetContent_List()
    {
        HideContent();
        Content.Show();
        ContentList.Show();
        ContentList.Clear();
    }

    private void ToggleVisible() =>
        SetVisible(!Main.Visible);

    private void Clear()
    {
        foreach (var button in _buttons)
        {
            button.QueueFree();
        }
        _buttons.Clear();

        foreach (var label in _categories.Values)
        {
            label.QueueFree();
        }
        _categories.Clear();
    }

    private void CreateActionButtons()
    {
        foreach (var action in Debug.RegisteredActions)
        {
            CreateAction(action);
        }
    }

    private Button CreateActionButton()
    {
        var button = ButtonPrefab.Duplicate() as Button;
        button.SetParent(ButtonPrefab.GetParent());
        button.Visible = true;
        _buttons.Add(button);
        return button;
    }

    private void CreateAction(DebugAction debug_action)
    {
        var button = CreateActionButton();
        button.Text = debug_action.Text;

        button.Pressed += () =>
        {
            debug_action.Action(this);
            SfxClick.Play();
        };

        button.MouseEntered += () =>
        {
            SfxHover.Play();
        };

        TryCreateCategory(debug_action);
        OrderActionButton(button, debug_action);
    }

    private void TryCreateCategory(DebugAction debug_action)
    {
        if (!_categories.ContainsKey(debug_action.Category))
        {
            CreateActionLabel(debug_action.Category);
        }
    }

    private void OrderActionButton(Button button, DebugAction debug_action)
    {
        var label = _categories[debug_action.Category];
        button.GetParent().MoveChild(button, label.GetIndex() + 1);
    }

    private Label CreateActionLabel(string text)
    {
        var instance = CategoryPrefab.Duplicate() as Label;
        instance.SetParent(CategoryPrefab.GetParent());
        instance.Text = text;
        instance.Visible = true;
        _categories.Add(text, instance);
        return instance;
    }

    public void PopupStringInput(string label, Action<string> onSuccess)
    {
        Main.Hide();
        InputPopup.Show();
        InputPopup.Clear();
        InputPopup.CreateStringInput("id", label);

        _onInputPopupSuccess = OnSuccess;

        void OnSuccess(Dictionary<string, string> results)
        {
            var result = results.FirstOrDefault().Value;
            onSuccess?.Invoke(result);
        }
    }
}
