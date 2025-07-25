using Godot;
using System.Collections;

public partial class CreditsView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/{nameof(CreditsView)}";
    public static CreditsView Instance => Singleton.Get<CreditsView>();

    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public Control CreditsControl;

    [Export]
    public AudioStreamPlayer BgmCredits;

    public int Ending { get; set; }

    private float _mul_speed = 1.0f;

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
        _mul_speed = holding ? 3.0f : 1.0f;
        AnimationPlayer.SpeedScale = _mul_speed;
    }

    public void AnimateAll()
    {
        HideViews();
        SetLocksEnabled(true);
        AmbienceController.Instance.StopAmbience();

        Coroutine.Start(Cr)
            .SetRunWhilePaused();

        IEnumerator Cr()
        {
            var anim_ending = Ending == 1 ? "ending1" : "ending2";
            yield return AnimateCredits();
            SetAchievement();
            yield return AnimationPlayer.PlayAndWaitForAnimation(anim_ending);
            MainMenuView.Instance.AnimateTransitionToMainMenu();
            yield return new WaitForSecondsUnscaled(1f);
            SetLocksEnabled(false);
            Hide();
        }
    }

    private Coroutine AnimateCredits()
    {
        return Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            BgmCredits.VolumeDb = -80f;
            BgmCredits.Play();
            BgmCredits.Fade(2f, 0f);

            var height = CreditsControl.Size.Y;
            var window_height = Scene.Root.Size.Y;
            var full_height = height + window_height;
            var start = Vector2.Zero;
            var end = start + Vector2.Up * full_height;
            var duration = 0.01f * full_height;

            var time = 0f;
            var time_end = duration;
            while (time < time_end)
            {
                var f = time / duration;
                CreditsControl.Position = start.Lerp(end, f);
                time += GameTime.DeltaTime * _mul_speed;
                yield return null;
            }

            BgmCredits.FadeOut(2f);
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

    private void SetAchievement()
    {
        if (Ending == 1)
        {
            SteamController.Instance.SetAchievement(AchievementIds.ENDING_A);
        }
        else if (Ending == 2)
        {
            SteamController.Instance.SetAchievement(AchievementIds.ENDING_B);
        }
    }
}
