public partial class CreditsScene : Scene
{
    public override void _Ready()
    {
        base._Ready();

        CreditsView.Instance.Show();
        CreditsView.Instance.AnimateCredits();
    }
}
