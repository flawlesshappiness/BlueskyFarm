using Godot;

[GlobalClass]
public partial class SoundInfo : Resource
{
    [Export]
    public SoundName SoundName;

    [Export(PropertyHint.File)]
    public string Path;
}
