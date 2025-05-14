using Godot;
using System.Collections;

public partial class NoteView : View
{
    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public Label TextLabel;

    public static NoteView Instance => Get<NoteView>();
    public override string Directory => $"{Paths.ViewDirectory}/{nameof(NoteView)}";

    private bool note_active;

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
    }

    private void RegisterDebugActions()
    {
        var category = "NOTE";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Show",
            Action = v => { v.Close(); SetText("DEBUG"); AnimateShow(); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Hide",
            Action = v => { v.Close(); AnimateHide(); }
        });
    }

    protected override void OnHide()
    {
        base.OnHide();
        SetLocks(false);
        note_active = false;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (PlayerInput.Interact.Released)
        {
            Close();
        }
    }

    private void Close()
    {
        if (!note_active) return;

        AnimateHide();
    }

    public void SetText(string text)
    {
        TextLabel.Text = Tr(text);
    }

    public void AnimateShow()
    {
        this.StartCoroutine(Cr, "transition");
        IEnumerator Cr()
        {
            SetLocks(true);
            Show();
            yield return AnimationPlayer.PlayAndWaitForAnimation("show");
            note_active = true;
        }
    }

    public void AnimateHide()
    {
        this.StartCoroutine(Cr, "transition");
        IEnumerator Cr()
        {
            yield return AnimationPlayer.PlayAndWaitForAnimation("hide");
            Hide();
        }
    }

    private void SetLocks(bool locked)
    {
        var id = nameof(NoteView);
        PauseView.Instance.ToggleLock.SetLock(id, locked);
        Player.Instance.MovementLock.SetLock(id, locked);
        Player.Instance.InteractLock.SetLock(id, locked);
        Player.Instance.LookLock.SetLock(id, locked);
        InventoryController.Instance.InventoryLock.SetLock(id, locked);
    }
}
