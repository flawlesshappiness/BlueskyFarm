using System.Linq;

public partial class ForgeMoldController : ResourceController<ForgeMoldCollection, ForgeMoldInfo>
{
    public static ForgeMoldController Instance => Singleton.Get<ForgeMoldController>();
    public override string Directory => "Forge";

    public ForgeMoldInfo GetInfo(string custom_id)
    {
        return Collection.Resources.FirstOrDefault(x => x.ItemCustomId == custom_id);
    }
}
