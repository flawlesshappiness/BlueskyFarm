using Godot;
using System.Linq;

public partial class CursorController : ResourceController<CursorCollection, CursorInfo>
{
    public static CursorController Instance => Singleton.Get<CursorController>();
    public override string Directory => $"{Paths.Modules}/Cursor";

    public CursorInfo GetInfo(string name)
    {
        return Collection.Resources.FirstOrDefault(x => x.Name == name);
    }

    public Texture2D GetCursorTexture(string name)
    {
        var info = GetInfo(name);
        return info?.Texture;
    }
}
