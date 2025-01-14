using System.Linq;

public partial class EnvironmentController : ResourceController<EnvironmentCollection, EnvironmentInfo>
{
    public static EnvironmentController Instance => Singleton.Get<EnvironmentController>();
    public override string Directory => "Environment";

    public void SetEnvironment(AreaNameType area)
    {
        var info = GetInfo(area);
        var scene = Scene.Current as GameScene;

        scene.WorldEnvironment.Environment = info.Environment;
        scene.DirectionalLight.LightColor = info.DirectionalLightColor;
    }

    public EnvironmentInfo GetInfo(AreaNameType area) => GetInfo(area.ToString());
    public EnvironmentInfo GetInfo(string area)
    {
        return Collection.Resources.FirstOrDefault(x => x.Area.ToString() == area);
    }
}
