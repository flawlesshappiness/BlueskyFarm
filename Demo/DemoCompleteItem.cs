public partial class DemoCompleteItem : Item
{
    public override void _Ready()
    {
        base._Ready();

        var view = View.Get<DemoView>();
        view.Show();
        view.AnimateDemoCompleted();
    }
}
