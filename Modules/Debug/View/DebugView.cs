using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DebugView : View
{
    public override string Directory => $"{Paths.Modules}/Debug/View";

    [Export]
    public DebugCategoryControl CategoryTemplate;

    [NodeName]
    public Control Main;

    [NodeName]
    public Control Content;

    [NodeName]
    public DebugInputPopup InputPopup;

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

    private Dictionary<string, DebugCategoryControl> _categories = new();
    private Action<Dictionary<string, string>> _onInputPopupSuccess;

    public override void _Ready()
    {
        base._Ready();
        Show();
        CategoryTemplate.Hide();
        HideContent();
        Close();

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

    private void ToggleVisible() => SetMenuVisible(!Main.Visible);
    public void Open() => SetMenuVisible(true);
    public void Close() => SetMenuVisible(false);
    public void SetMenuVisible(bool visible)
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


    private void Clear()
    {
        foreach (var category in _categories.Values)
        {
            category.QueueFree();
        }
        _categories.Clear();
    }

    private void CreateActionButtons()
    {
        foreach (var action in Debug.RegisteredActions)
        {
            var category = GetOrCreateCategory(action.Id, action.Category);
            category.CreateButton(action.Text, action.Action);
        }
    }

    private DebugCategoryControl GetOrCreateCategory(string id, string text)
    {
        if (string.IsNullOrEmpty(id))
        {
            id = text;
        }

        if (!_categories.ContainsKey(id))
        {
            var category = CategoryTemplate.Duplicate() as DebugCategoryControl;
            category.SetParent(CategoryTemplate.GetParent());
            category.Show();
            category.CategoryLabel.Text = text;
            _categories.Add(id, category);
        }

        return _categories[id];
    }

    private void OrderActionButton(Button button, DebugAction debug_action)
    {
        var label = _categories[debug_action.Category];
        button.GetParent().MoveChild(button, label.GetIndex() + 1);
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
