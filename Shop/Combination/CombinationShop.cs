using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class CombinationShop : Node3DScript
{
    [NodeName]
    public Touchable Button;

    [NodeName]
    public CombinationPipe Pipe1;

    [NodeName]
    public CombinationPipe Pipe2;

    [NodeName]
    public CombinationPipe Pipe3;

    [NodeName]
    public CombinationPipe Pipe4;

    public List<CombinationPipe> Pipes => _pipes ?? (_pipes = new() { Pipe1, Pipe2, Pipe3, Pipe4 });
    private List<CombinationPipe> _pipes;

    public override void _Ready()
    {
        base._Ready();

        Button.OnTouched += ButtonTouched;
    }

    private void ButtonTouched()
    {
        BuyItem();
    }

    private List<CraftingMaterialType> GetMaterialCombination()
    {
        return Pipes
            .Where(x => x.CurrentMaterial != null)
            .Select(x => x.CurrentMaterial.Type)
            .ToList();
    }

    private void ConsumeCurrentCombination()
    {
        Pipes.ForEach(x => x.ConsumeMaterial());
    }

    private void DetachCurrentCombination()
    {
        Pipes.ForEach(x => x.DetachMaterial());
    }

    private void BuyItem()
    {
        var combination = GetMaterialCombination();
        var info = ShopController.Instance.GetShopItem(combination);
        if (info == null)
        {
            DetachCurrentCombination();

            SoundController.Instance.Play("sfx_shop_error", new SoundSettings3D
            {
                Bus = SoundBus.SFX,
                Position = GlobalPosition
            });
        }
        else
        {
            var item = ShopController.Instance.CreateShopItem(info);
            item.GlobalPosition = GlobalPosition + new Vector3(0, 50, 0);

            ConsumeCurrentCombination();

            SoundController.Instance.Play("sfx_shop_accept", new SoundSettings3D
            {
                Bus = SoundBus.SFX,
                Position = GlobalPosition
            });
        }
    }
}
