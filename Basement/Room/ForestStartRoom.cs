using Godot;

public partial class ForestStartRoom : Node3DScript
{
    [Export]
    public Weed_Thorns WeedBlockade;

    public override void _Ready()
    {
        base._Ready();

        if (Data.Game.Flag_ForestBlockadeRemoved)
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
        Data.Game.Flag_ForestBlockadeRemoved = true;
        Data.Game.Save();
    }
}
