using Godot;
using System.Text;

public partial class GDHelper
{
    public static T Instantiate<T>(string scene_path) where T : Node
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
        return Instantiate<T>(packed_scene);
    }

    public static T Instantiate<T>(PackedScene packed_scene) where T : Node
    {
        var node = packed_scene.Instantiate();
        Scene.Root.AddChild(node);
        var script = node.GetNodeInChildren<T>();
        return script;
    }
}
