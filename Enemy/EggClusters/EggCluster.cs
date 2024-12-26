public partial class EggCluster : Node3DScript
{
    [NodeName]
    public PlayerArea SlowArea;

    private string FxId => $"{nameof(EggCluster)}_{GetInstanceId()}";

    public override void _Ready()
    {
        base._Ready();

        SlowArea.PlayerEntered += PlayerEntered;
        SlowArea.PlayerExited += PlayerExited;
    }

    private void PlayerEntered(Player player)
    {
        player.SetMoveSpeedMultiplier(FxId, 0.4f);
    }

    private void PlayerExited(Player player)
    {
        player.RemoveMoveSpeedMultiplier(FxId);
    }
}
