using Godot;
using Godot.Collections;

[GlobalClass]
public partial class AmbienceGroupInfo : Resource
{
    [Export]
    public string Area;

    [Export]
    public float DelayMin = 10f;

    [Export]
    public float DelayMax = 15f;

    [Export]
    public Array<SoundInfo> Infos;
}
