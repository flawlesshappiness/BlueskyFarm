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

            view.ContentSearch.AddItem("Basement", () => { view.Close(); Scene.Goto("Screenshot_001"); });
            view.ContentSearch.AddItem("Basement Corridor", () => { view.Close(); Scene.Goto("Screenshot_005"); });
            view.ContentSearch.AddItem("Forest", () => { view.Close(); Scene.Goto("Screenshot_002"); });
            view.ContentSearch.AddItem("Cult Hallway", () => { view.Close(); Scene.Goto("Screenshot_003"); });
            view.ContentSearch.AddItem("Mine Corridor", () => { view.Close(); Scene.Goto("Screenshot_004"); });
            view.ContentSearch.UpdateButtons();
        }
    }
}
