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

    public bool Repaired => Data.Game.Flag_BedRepaired;
    private string DebugCategory => "FARM - BED";

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();

        Touchable.OnTouched += Touched;
        UnlockArea.OnItemEntered += UnlockArea_ItemEntered;

        UnlockGroup.SetUnlocked(Data.Game.Flag_BedRepaired);
        UnlockArea.SetEnabled(!Data.Game.Flag_BedRepaired);
    }

    private void RegisterDebugActions()
    {
        UnlockGroup.RegisterDebugActions(DebugCategory);

        Debug.RegisterAction(new DebugAction
        {
            Id = DebugCategory,
            Category = DebugCategory,
            Text = "Set unlocked",
            Action = v => { Data.Game.Flag_BedRepaired = true; UnlockGroup.SetUnlocked(); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Id = DebugCategory,
            Category = DebugCategory,
            Text = "Clear data",
            Action = v => Data.Game.Flag_BedRepaired = false
        });
    }

    private void UnlockArea_ItemEntered(Item item)
    {
        if (Repaired) return;

        UnlockArea.Disable();
        Data.Game.Flag_BedRepaired = true;

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

            GameView.Instance.CreateText(new CreateTextSettings
            {
                Id = "bed_cant_sleep_" + GetInstanceId(),
                Text = "##BED_CANT_SLEEP##",
                Target = Touchable,
                Offset = new Vector3(0, 0.2f, 0),
                Duration = 5.0f,
            });
        }
    }

    private void Sleep()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var view = View.Get<GameView>();

            yield return view.TransitionStartCr(nameof(FarmBed));

            GrowPlants();
            yield return new WaitForSeconds(2f);

            yield return view.TransitionEndCr(nameof(FarmBed));
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
