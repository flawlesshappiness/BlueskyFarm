using System.Linq;

public partial class SolidMaterialController : ResourceController<SolidMaterialCollection, SolidMaterialInfo>
{
    public static SolidMaterialController Instance => Singleton.Get<SolidMaterialController>();
    public override string Directory => "SolidMaterial";

    public SolidMaterialInfo GetInfo(SolidMaterialType type)
    {
        return Collection.Resources.FirstOrDefault(x => x.Type == type) ?? Collection.DefaultMaterial;
    }
}
