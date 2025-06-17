using Godot;
using System.Collections;

public partial class FarmBed : Node3D
{
    [Export]
    public Touchable Touchable;

    [Export]
    public UnlockableGroup UnlockGroup;

    [Export]
    public ItemArea UnlockArea;

    [Export]
    public Texture2D HoverIconDestroyed;

    [Export]
    public Texture2D HoverIconRepaired;

    public bool Repaired => GameFlagIds.FarmBedRepaired.IsTrue();
    private string DebugCategory => "FARM - BED";

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();

        Touchable.OnTouched += Touched;
        UnlockArea.OnItemEntered += UnlockArea_ItemEntered;

        Touchable.HoverIcon = Repaired ? HoverIconRepaired : HoverIconDestroyed;

        UnlockGroup.SetUnlocked(Repaired);
        UnlockArea.SetEnabled(!Repaired);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Debug.RemoveActions(DebugCategory);
    }

    private void RegisterDebugActions()
    {
        UnlockGroup.RegisterDebugActions(DebugCategory);

        Debug.RegisterAction(new DebugAction
        {
            Id = DebugCategory,
            Category = DebugCategory,
            Text = "Set unlocked",
            Action = v => { GameFlagIds.FarmBedRepaired.SetTrue(); UnlockGroup.SetUnlocked(); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Id = DebugCategory,
            Category = DebugCategory,
            Text = "Clear data",
            Action = v => { GameFlagIds.FarmBedRepaired.SetFalse(); UnlockArea.Enable(); }
        });
    }

    private void UnlockArea_ItemEntered(Item item)
    {
        if (Repaired) return;

        UnlockArea.Disable();
        GameFlagIds.FarmBedRepaired.SetTrue();

        Touchable.HoverIcon = HoverIconRepaired;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            SoundController.Instance.Play("sfx_throw_light", item.GlobalPosition);
            yield return item.AnimateDisappearAndQueueFree();
            yield return UnlockGroup.AnimateUnlock();
            Data.Game.Save();
        }
    }

    private void Touched()
    {
        if (Repaired)
        {
            StartSleep();
        }
        else
        {
            SoundController.Instance.Play("sfx_throw_light");
        }
    }

    private void StartSleep()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var view = View.Get<GameView>();

            Data.Game.Save();

            yield return view.TransitionStartCr(new TransitionSettings
            {
                Duration = 2f,
                GaussianBlur = 20f,
                GaussianBlurStartDuration = 1f
            });

            yield return new WaitForSeconds(1f);

            bool is_dream = true;
            if (is_dream)
            {
                StartDream();
                GrowPlants();
            }
            else
            {
                yield return new WaitForSeconds(1f);
                yield return view.TransitionEndCr(new TransitionSettings
                {
                    Duration = 2f,
                    GaussianBlur = 20f,
                    GaussianBlurStartDuration = 1f
                });
            }
        }
    }

    private void StartDream()
    {
        DreamController.Instance.StartRandomDream();
    }

    private void GrowPlants()
    {
        Data.Game.PlantAreas.ForEach(area => area.TimeLeft = 0);
    }
}
