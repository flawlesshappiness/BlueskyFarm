using Godot;

[GlobalClass]
public partial class ApplicationInfo : Resource
{
    private static ApplicationInfo _instance;
    public static ApplicationInfo Instance => _instance ?? (_instance = GD.Load<ApplicationInfo>($"{Paths.Modules}/Application/{nameof(ApplicationInfo)}.tres"));

    [Export]
    public string Version { get; set; }

    [Export]
    public bool Release { get; set; }
}
