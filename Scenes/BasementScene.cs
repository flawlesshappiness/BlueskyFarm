public partial class BasementScene : Scene
{
    protected override void Initialize()
    {
        base.Initialize();
        BasementController.Instance.DebugGenerateBasement();
    }
}
