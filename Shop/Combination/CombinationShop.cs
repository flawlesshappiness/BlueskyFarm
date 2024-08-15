using Godot;

public partial class CombinationShop : Node3DScript
{
    [NodeName]
    public Touchable InputCancel;

    [NodeName]
    public Touchable InputAccept;

    [NodeType]
    public CombinationSelector Selector;

    [NodeType]
    public CombinationDisplay Display;

    [NodeType]
    public CombinationPrice Price;

    private ShopItemInfo _selected_item;

    public override void _Ready()
    {
        base._Ready();

        InputCancel.OnTouched += PressCancel;
        InputAccept.OnTouched += PressAccept;
        Selector.OnCombinationChanged += Display.UpdateDisplay;
        Selector.OnCombinationChanged += _ => Price.Clear();

        Clear();
    }

    private void PressAccept()
    {
        if (Price.IsShowingPrice)
        {
            BuySelectedItem();
            Clear();
        }
        else
        {
            var combination = Selector.CurrentCombination;
            Selector.Clear();
            SelectItem(combination);
        }
    }

    private void PressCancel()
    {
        Clear();

        SoundController.Instance.Play("sfx_shop_cancel", new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = GlobalPosition
        });
    }

    private void Clear()
    {
        _selected_item = null;
        Selector.Clear();
        Price.Clear();
    }

    private void SelectItem(string combination)
    {
        _selected_item = ShopController.Instance.GetShopItem(combination);

        if (_selected_item != null)
        {
            Price.SetPrice(_selected_item.Price);

            SoundController.Instance.Play("sfx_shop_select", new SoundSettings3D
            {
                Bus = SoundBus.SFX,
                Position = GlobalPosition
            });
        }
        else
        {
            Clear();

            SoundController.Instance.Play("sfx_shop_error", new SoundSettings3D
            {
                Bus = SoundBus.SFX,
                Position = GlobalPosition
            });
        }
    }

    private bool CanBuySelectedItem()
    {
        if (_selected_item == null) return false;
        if (!ShopController.Instance.CanAfford(_selected_item)) return false;
        return true;
    }

    private void BuySelectedItem()
    {
        if (CanBuySelectedItem())
        {
            ShopController.Instance.PurchaseShopItem(_selected_item);
            var item = ShopController.Instance.CreateShopItem(_selected_item);

            item.GlobalPosition = GlobalPosition + new Vector3(0, 50, 0);

            SoundController.Instance.Play("sfx_shop_accept", new SoundSettings3D
            {
                Bus = SoundBus.SFX,
                Position = GlobalPosition
            });
        }
        else
        {
            SoundController.Instance.Play("sfx_shop_error", new SoundSettings3D
            {
                Bus = SoundBus.SFX,
                Position = GlobalPosition
            });
        }
    }
}
