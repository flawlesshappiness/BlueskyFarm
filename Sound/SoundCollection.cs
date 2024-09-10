using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class SoundCollection : ResourceCollection<SoundInfo>
{
    private Dictionary<string, AudioStream> _sounds = new();

    protected override void OnLoad()
    {
        base.OnLoad();
        _sounds = LoadResources<AudioStream>("res://Sound/Sounds");
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
}
