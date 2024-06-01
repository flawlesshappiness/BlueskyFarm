using Godot;
using System.Text;

public partial class GDHelper
{
    public static T Instantiate<T>(string scene_path, Node parent = null) where T : Node => Instantiate(scene_path, parent) as T;
    public static Node Instantiate(string scene_path, Node parent = null)
    {
        Debug.TraceMethod(scene_path);

        var prefix = "res://";
        var ext = ".tscn";
        var sb = new StringBuilder();
        if (!scene_path.StartsWith(prefix)) sb.Append(prefix);
        sb.Append(scene_path);
        if (!scene_path.EndsWith(ext)) sb.Append(ext);
        var path = sb.ToString();

        var packed_scene = GD.Load(path) as PackedScene;
        return Instantiate(packed_scene, parent);
    }

    public static T Instantiate<T>(PackedScene packed_scene, Node parent = null) where T : Node => Instantiate(packed_scene, parent) as T;
    public static Node Instantiate(PackedScene packed_scene, Node parent = null)
    {
        var node = packed_scene.Instantiate();
        parent ??= Scene.Root;
        parent.AddChild(node);
        return node;
    }
}
