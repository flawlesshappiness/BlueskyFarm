using Godot;
using System.Collections.Generic;
using System.IO;

[GlobalClass]
public partial class SoundCollection : ResourceCollection<SoundInfo>
{
    private Dictionary<string, AudioStream> _sounds = new();

    protected override void OnLoad()
    {
        base.OnLoad();
        LoadSounds();
    }

    private void LoadSounds()
    {
        var path = "res://Sound/Sounds";
        var dir = DirAccess.Open(path);
        var files = dir.GetFiles();

        foreach (var file in files)
        {
            try
            {
                var path_file = $"{path}/{file}";
                var ext = path_file.GetExtension();
                if (ext == "import") continue;

                var resource = GD.Load<AudioStream>(path_file);
                var filename = Path.GetFileName(path_file).Replace($".{ext}", "");
                _sounds.Add(filename, resource);
            }
            catch
            {
                continue;
            }
        }
    }

    public AudioStream GetSound(string filename)
    {
        filename = GetFilename(filename);

        if (_sounds.ContainsKey(filename))
        {
            return _sounds[filename];
        }

        Debug.LogError("Failed to load sound with name: " + filename);
        return null;
    }

    private string GetFilename(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        var ext = path.GetExtension();
        return Path.GetFileName(path).Replace($".{ext}", "");
    }
}
