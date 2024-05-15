using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DebugContentSearch : ControlScript
{
    [NodeName("Title")]
    public Label TitleLabel;

    [NodeName("SearchField")]
    public LineEdit SearchField;

    [NodeName("ResultButtonPrefab")]
    public Button ResultButtonPrefab;

    public string Title { set => TitleLabel.Text = value; }

    private Dictionary<string, Action> _items = new();

    private List<Button> _buttons = new();

    public override void _Ready()
    {
        base._Ready();

        VisibilityChanged += OnVisibilityChanged;
        SearchField.TextChanged += OnTextChanged;

        ResultButtonPrefab.Visible = false;
    }

    private void OnVisibilityChanged()
    {
        if (Visible)
        {
            OnTextChanged(string.Empty);
        }
    }

    private void OnTextChanged(string text)
    {
        ClearButtons();

        var items = _items.Where(item => item.Key.ToLower().Contains(text.ToLower()));
        foreach (var item in items)
        {
            var button = CreateButton();
            button.Text = item.Key;
            button.Pressed += item.Value;
        }
    }

    private Button CreateButton()
    {
        var instance = ResultButtonPrefab.Duplicate() as Button;
        instance.SetParent(ResultButtonPrefab.GetParent());
        instance.Visible = true;
        _buttons.Add(instance);
        return instance;
    }

    private void ClearButtons()
    {
        foreach (var button in _buttons)
        {
            button.QueueFree();
        }

        _buttons.Clear();
    }

    public void AddItem(string text, Action action)
    {
        _items.TryAdd(text, action);
    }

    public void ClearItems()
    {
        _items.Clear();
    }
}
