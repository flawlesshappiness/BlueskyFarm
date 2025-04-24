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
            Sleep();
        }
        else
        {
            SoundController.Instance.Play("sfx_throw_light");

            /*
            GameView.Instance.CreateText(new CreateTextSettings
            {
                Id = "bed_cant_sleep_" + GetInstanceId(),
                Text = "##BED_CANT_SLEEP##",
                Target = Touchable,
                Offset = new Vector3(0, 0.2f, 0),
                Duration = 3.0f,
            });
            */
        }
    }

    private void Sleep()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var view = View.Get<GameView>();

            yield return view.TransitionStartCr(new TransitionSettings
            {
                Duration = 2f,
                GaussianBlur = 20f,
                GaussianBlurStartDuration = 1f
            });

            GrowPlants();
            yield return new WaitForSeconds(2f);

            yield return view.TransitionEndCr(new TransitionSettings
            {
                Duration = 2f,
                GaussianBlur = 20f,
                GaussianBlurStartDuration = 1f
            });
        }
    }

    private void GrowPlants()
    {
        var scene = Scene.Current as FarmScene;

        foreach (var plant_area in scene.PlantAreas)
        {
            plant_area.CompleteSeed();
        }
    }
}
