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
        if (info == null)
        {
            Debug.TraceMethod();
            Debug.LogError($"Failed to get BlueprintInfo with id: {Data.Blueprint.Id}");
            return;
        }

        var has_override = !string.IsNullOrEmpty(info.OverrideResultName);
        var has_result_item = info.ResultItemInfo != null;

        if (has_override || has_result_item)
        {
            var result_name = has_override ? info.OverrideResultName : info.ResultItemInfo.ItemName;
            HoverText = $"{Tr("##BLUEPRINT##")}: {result_name}";
        }
        else
        {
            HoverText = $"{Tr("##BLUEPRINT##")}";
        }
    }
}
