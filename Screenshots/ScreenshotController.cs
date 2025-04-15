public partial class ScreenshotController : SingletonController
{
    public override string Directory => "Screenshots";

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
    }

    private void RegisterDebugActions()
    {
        var category = "SCREENSHOTS";

        Debug.RegisterAction(new DebugAction
        {
            Id = category,
            Category = category,
            Text = "Goto",
            Action = Goto
        });

        void Goto(DebugView view)
        {
            view.HideContent();
            view.SetContent_Search();

            view.ContentSearch.AddItem("Screenshot_001", () => { view.Close(); Scene.Goto("Screenshot_001"); });
            view.ContentSearch.UpdateButtons();
        }
    }
}
