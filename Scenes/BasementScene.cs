public partial class BasementScene : Scene
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        BasementController.Instance.DebugGenerateBasement();
    }
}
