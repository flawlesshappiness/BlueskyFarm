using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class LimbIK : SkeletonModifier3D
{
    [Export(PropertyHint.Enum, " ")]
    public string Bone;

    public override void _ValidateProperty(Dictionary property)
    {
        base._ValidateProperty(property);

        if (property["name"].AsString() == nameof(Bone))
        {
            var skeleton = GetSkeleton();
            if (IsInstanceValid(skeleton))
            {
                property["hint_string"] = skeleton.GetConcatenatedBoneNames();
            }
        }
    }

    public override void _ProcessModification()
    {
        base._ProcessModification();

        var skeleton = GetSkeleton();
        if (!IsInstanceValid(skeleton)) return;

        var idx_bone = skeleton.FindBone(Bone);
        if (idx_bone < 0) return;

        var trans = Transform;
        skeleton.SetBoneGlobalPose(idx_bone, trans);
    }
}
