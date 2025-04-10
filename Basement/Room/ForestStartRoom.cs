using Godot;

public partial class ForestStartRoom : Node3DScript
{
    [Export]
    public Weed_Thorns WeedBlockade;

    public override void _Ready()
    {
        base._Ready();

        WeedBlockade.Touchable.OnTouched += Touched_WeedBlockade;

        if (GameFlagIds.ForestBlockadeRemoved.IsTrue())
        {
            RemoveWeeds();
        }
        else
        {
            InitializeWeeds();
        }
    }

    private void InitializeWeeds()
    {
        WeedBlockade.OnWeedCut += OnWeedCut;
    }

    private void RemoveWeeds()
    {
        WeedBlockade.SetCut(true);
    }

    private void OnWeedCut()
    {
        GameFlagIds.ForestBlockadeRemoved.SetTrue();
        Data.Game.Save();
    }

    private void Touched_WeedBlockade()
    {
        DialogueFlags.SetFlagMin(DialogueFlags.FrogForestWeeds, 1);
    }
}
