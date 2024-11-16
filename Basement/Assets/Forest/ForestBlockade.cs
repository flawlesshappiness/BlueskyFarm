using Godot;
using System.Linq;

public partial class ForestBlockade : Touchable
{
    private int _weed_count;

    public override void _Ready()
    {
        base._Ready();

        if (Data.Game.Flag_ForestBlockadeRemoved)
        {
            RemoveBlockade();
        }
        else
        {
            InitializeWeeds();
        }
    }

    private void InitializeWeeds()
    {
        var weeds = this.GetNodesInChildren<Weed>();
        _weed_count = weeds.Count();

        foreach (var weed in weeds)
        {
            weed.OnWeedCutFinished += WeedCut;
        }
    }

    private void WeedCut()
    {
        _weed_count--;

        if (_weed_count <= 0)
        {
            RemoveBlockade();
        }
    }

    private void RemoveBlockade()
    {
        Data.Game.Flag_ForestBlockadeRemoved = true;
        QueueFree();
    }

    protected override void Touched()
    {
        base.Touched();
        var view = View.Get<GameView>();
        DialogueController.Instance.SetNode("##REQ_WEEDS_TOOL_001##");
    }
}
