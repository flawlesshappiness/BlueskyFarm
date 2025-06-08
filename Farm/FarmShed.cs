using Godot;
using System.Collections;

public partial class FarmShed : Node3D
{
    [Export]
    public UnlockableGroup UnlockGroup;

    [Export]
    public ItemArea UnlockItemArea;

    private string DebugCategory = "FARM - SHED";

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
        UnlockItemArea.OnItemEntered += ItemEntered;

        Load();
    }

    private void Load()
    {
        UnlockGroup.SetUnlocked(GameFlagIds.FarmShedRepaired.IsTrue());
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
            Text = "Clear data",
            Action = v => GameFlagIds.FarmShedRepaired.SetFalse()
        });
    }

    private void ItemEntered(Item item)
    {
        UnlockItemArea.Disable();
        GameFlagIds.FarmShedRepaired.SetTrue();

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            SoundController.Instance.Play("sfx_throw_light", item.GlobalPosition);
            yield return item.AnimateDisappearAndQueueFree();
            yield return UnlockGroup.AnimateUnlock();
            SteamController.Instance.SetAchievement(AchievementIds.SHED);
        }
    }
}
