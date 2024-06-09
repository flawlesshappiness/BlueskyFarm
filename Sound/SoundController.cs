using System.Linq;

public partial class SoundController : ResourceController<SoundCollection, SoundInfo>
{
    public override string Directory => "Sound";
    public static SoundController Instance => Singleton.Get<SoundController>();

    public SoundInfo GetInfo(SoundName name) => Collection.Resources.FirstOrDefault(x => x.SoundName == name);
}
