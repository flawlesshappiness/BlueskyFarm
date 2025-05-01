using Godot;

public partial class ProfilesView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/{nameof(ProfilesView)}";
    public static ProfilesView Instance => View.Get<ProfilesView>();

    [Export]
    public Button BackButton;

    [Export]
    public ProfileControl Profile1;

    [Export]
    public ProfileControl Profile2;

    [Export]
    public ProfileControl Profile3;

    [Export]
    public DeleteProfilePopup DeleteProfilePopup;

    private int _profile_to_delete = 3;

    public override void _Ready()
    {
        base._Ready();
        BackButton.Pressed += ClickBack;
        Profile1.OnProfileSelected += ClickProfile;
        Profile2.OnProfileSelected += ClickProfile;
        Profile3.OnProfileSelected += ClickProfile;

        Profile1.OnProfileDeletePressed += ClickProfileDelete;
        Profile2.OnProfileDeletePressed += ClickProfileDelete;
        Profile3.OnProfileDeletePressed += ClickProfileDelete;

        DeleteProfilePopup.OnDelete += ClickProfileDeleteConfirm;
        DeleteProfilePopup.Hide();
    }

    protected override void OnShow()
    {
        base.OnShow();
        LoadProfiles();
    }

    private void LoadProfiles()
    {
        Profile1.Load();
        Profile2.Load();
        Profile3.Load();
    }

    private void ClickBack()
    {
        Hide();
        MainMenuView.Instance.Show();
    }

    private void ClickProfile(int profile)
    {
        Data.Options.SelectedGameProfile = profile;
        Data.Options.Save();
        ClickBack();
    }

    private void ClickProfileDelete(int profile)
    {
        _profile_to_delete = profile;
        DeleteProfilePopup.UpdateTitle(_profile_to_delete);
        DeleteProfilePopup.Show();
    }

    private void ClickProfileDeleteConfirm()
    {
        GameProfileController.Instance.DeleteGameProfile(_profile_to_delete);
        LoadProfiles();
    }
}
