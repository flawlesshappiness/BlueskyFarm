using Godot;
using System.Collections;

public partial class CreditsView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/{nameof(CreditsView)}";
    public static CreditsView Instance => Singleton.Get<CreditsView>();

    [Export]
    public AnimationPlayer AnimationPlayer;

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
    }

    private void RegisterDebugActions()
    {
        var category = "CREDITS";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Show credits",
            Action = ShowCredits
        });

        void ShowCredits(DebugView view)
        {
            view.Close();
            Scene.Goto(nameof(CreditsScene));
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        var holding = PlayerInput.Jump.Held || PlayerInput.Interact.Held;
        var speed = holding ? 3.0f : 1.0f;
        AnimationPlayer.SpeedScale = speed;
    }

    public void AnimateCredits()
    {
        HideViews();
        SetLocksEnabled(true);
        AmbienceController.Instance.StopAmbience();

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return AnimationPlayer.PlayAndWaitForAnimation("credits");
            yield return AnimationPlayer.PlayAndWaitForAnimation("ending");
            MainMenuView.Instance.AnimateTransitionToMainMenu();
            yield return new WaitForSeconds(1f);
            SetLocksEnabled(false);
            Hide();
        }
    }

    private void SetLocksEnabled(bool enabled)
    {
        var id = nameof(CreditsView);
        PauseView.Instance.ToggleLock.SetLock(id, enabled);
    }

    private void HideViews()
    {
        GameView.Instance.Hide();
        MainMenuView.Instance.Hide();
    }
}
