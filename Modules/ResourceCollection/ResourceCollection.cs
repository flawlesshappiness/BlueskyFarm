using Godot;
using System.Collections.Generic;
using System.IO;

public partial class ResourceCollection<T> : Resource where T : Resource
{
    private List<T> _resources;
    public List<T> Resources => _resources;

    public static C Load<C>(string path) where C : ResourceCollection<T>
    {
        Debug.TraceMethod(path);
        Debug.Indent++;

        var collection = GD.Load<C>(path);
        var filename_collection = Path.GetFileName(path);
        var path_dir = "res://" + path.Replace(filename_collection, "");
        var dir = DirAccess.Open(path_dir);
        var files = dir.GetFiles();

        var resources = new List<T>();
        foreach (var file in files)
        {
            try
            {
                var filename = file.Replace(".remap", "");
                var path_file = $"{path_dir}{filename}";
                var resource = GD.Load<T>(path_file);
                Debug.Trace("Resource loaded: " + path_file);
                resources.Add(resource);
            }
            catch
            {
                continue;
            }
        }

        collection.SetResources(resources);
        Debug.Indent--;
        return collection;
    }

    private void SetResources(List<T> resources) => _resources = resources;
}
