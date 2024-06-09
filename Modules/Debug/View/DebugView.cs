using Godot;
using Godot.Collections;

public partial class DebugView : View
{
    [NodeName(nameof(Main))]
    public Control Main;

    [NodeName(nameof(Content))]
    public Control Content;

    [NodeName(nameof(ButtonPrefab))]
    public Button ButtonPrefab;

    [NodeName(nameof(CategoryPrefab))]
    public Label CategoryPrefab;

    [NodeName(nameof(ContentSearch))]
    public DebugContentSearch ContentSearch;

    [NodeName(nameof(ContentList))]
    public DebugContentList ContentList;

    [NodeName(nameof(SfxClick))]
    public AudioStreamPlayer SfxClick;

    [NodeName(nameof(SfxHover))]
    public AudioStreamPlayer SfxHover;

    [NodeName(nameof(SfxOpen))]
    public AudioStreamPlayer SfxOpen;

    private Dictionary<string, Label> _categories = new();

    public override string Directory => $"{Paths.Modules}/Debug/View";

    public override void _Ready()
    {
        base._Ready();
        Visible = true;
        ButtonPrefab.Visible = false;
        CategoryPrefab.Visible = false;
        HideContent();
        SetVisible(false);

        CreateActionButtons();
        Debug.OnActionAdded += CreateAction;
        Debug.RegisterDebugActions();
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

        var parent = GetParent();
        var lock_name = nameof(DebugView);
        if (visible)
        {
            MouseVisibility.Instance.Lock.AddLock(lock_name);
            Scene.PauseLock.AddLock(lock_name);

            var idx_max = parent.GetChildCount() - 1;
            parent.MoveChild(this, idx_max);
        }
        else
        {
            MouseVisibility.Instance.Lock.RemoveLock(lock_name);
            Scene.PauseLock.RemoveLock(lock_name);

            parent.MoveChild(this, 0);

            HideContent();

        }
    }

    public void HideContent()
    {
        Content.Visible = false;
        ContentSearch.Visible = false;
        ContentList.Visible = false;
    }

    private void ToggleVisible() =>
        SetVisible(!Main.Visible);

    private void CreateActionButtons()
    {
        foreach (var action in Debug.RegisteredActions)
        {
            CreateAction(action);
        }
    }

    private Button CreateActionButton()
    {
        var instance = ButtonPrefab.Duplicate() as Button;
        instance.SetParent(ButtonPrefab.GetParent());
        instance.Visible = true;
        return instance;
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
}
