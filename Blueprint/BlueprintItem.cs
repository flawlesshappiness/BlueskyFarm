public partial class BlueprintItem : Item
{
    protected override void Initialize()
    {
        base.Initialize();
        UpdateInteractText();
    }

    private void UpdateInteractText()
    {
        if (Data?.Blueprint == null) return;

        var info = BlueprintController.Instance.GetInfo(Data.Blueprint.Id);
        InteractableText = $"{Tr("##BLUEPRINT##")}: {info.ResultItemInfo.ItemName}";
    }
}
