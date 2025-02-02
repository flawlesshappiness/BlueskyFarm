using Godot;

[GlobalClass]
public partial class AmbienceArea : PlayerArea
{
    [Export]
    public AreaNameType Area;

    protected override void PlayerEntered(Player player)
    {
        base.PlayerEntered(player);
        AmbienceController.Instance.StartAmbience(Area);
    }
}
