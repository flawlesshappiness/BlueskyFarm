using Godot;
using System;
using System.Text;

public partial class GDHelper
{
    public static T Instantiate<T>(string scene_path) where T : Node => Instantiate(scene_path, typeof(T)) as T;
    public static Node Instantiate(string scene_path, Type type)
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
        return Instantiate(packed_scene);
    }

    public static T Instantiate<T>(PackedScene packed_scene) where T : Node => Instantiate(packed_scene) as T;
    public static Node Instantiate(PackedScene packed_scene)
    {
        var node = packed_scene.Instantiate();
        Scene.Root.AddChild(node);
        return node;
    }
}
