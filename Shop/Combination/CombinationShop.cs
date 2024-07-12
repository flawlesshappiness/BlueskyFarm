using Godot;
using System.Collections;

public partial class CombinationShop : Node3DScript
{
    [NodeName]
    public ITouchable InputCancel;

    [NodeName]
    public ITouchable InputAccept;

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
            SelectItem(Selector.CurrentCombination);
            Selector.Clear();
        }
    }

    private void PressCancel()
    {
        Clear();
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
        }
        else
        {
            Clear();
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

            AnimateCoins();
        }
        else
        {
            // Play sound to show it didn't work
        }
    }

    private void AnimateCoins()
    {
        Coroutine.Start(CoinsCr);

        IEnumerator CoinsCr()
        {
            for (int i = 0; i < 10; i++)
            {
                Coroutine.Start(CoinCr);
                yield return new WaitForSeconds(0.02f);
            }
        }

        IEnumerator CoinCr()
        {
            var info = ItemController.Instance.Collection.Coin;
            var coin = ItemController.Instance.CreateItem(info, track_item: false);

            coin.SetCollisionEnabled(false);
            coin.Freeze = true;
            coin.GlobalPosition = Price.PriceLabel.GlobalPosition;
            coin.GlobalRotation = new Vector3(90, 0, 0);

            var rng = new RandomNumberGenerator();
            var start_position = coin.GlobalPosition + new Vector3(rng.RandfRange(-1, 1), 0, rng.RandfRange(-1, 1));
            var end_position = start_position + new Vector3(0, 10, 0);
            var curve = Curves.EaseInOutQuint;

            coin.GlobalPosition = start_position;

            yield return LerpEnumerator.Lerp01(2f, f =>
            {
                coin.GlobalPosition = start_position.Lerp(end_position, curve.Evaluate(f));
            });

            coin.QueueFree();
        }
    }
}
