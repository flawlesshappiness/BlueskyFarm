using System.Linq;

public partial class BlueprintController : ResourceController<BlueprintCollection, BlueprintInfo>
{
    public static BlueprintController Instance => Singleton.Get<BlueprintController>();
    public override string Directory => "Blueprint";

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
    }

    private void RegisterDebugActions()
    {
        var category = "BLUEPRINT";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Set blueprint",
            Action = SetBlueprint
        });

        void SetBlueprint(DebugView view)
        {
            view.SetContent_Search();

            foreach (var info in Collection.Resources)
            {
                view.ContentSearch.AddItem(info.Id, () => Select(info));
            }
            view.ContentSearch.UpdateButtons();

            void Select(BlueprintInfo info)
            {
                var data = InventoryController.Instance.GetSelectedItem();
                if (data == null) return;

                data.Blueprint = new BlueprintData
                {
                    Id = info.Id
                };

                view.HideContent();
            }
        }

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Clear crafted blueprints data",
            Action = v => Data.Game.BlueprintCraftedCounts = new()
        });
    }

    public BlueprintInfo GetInfo(string id)
    {
        return Collection.Resources.FirstOrDefault(x => x.Id == id);
    }

    public Item CreateBlueprintRoll(string id)
    {
        var item_bp = ItemController.Instance.CreateItem("Blueprint");
        item_bp.Data.Blueprint = new BlueprintData
        {
            Id = id
        };

        return item_bp;
    }
}
