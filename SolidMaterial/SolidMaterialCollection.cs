using Godot;

[GlobalClass]
public partial class SolidMaterialCollection : ResourceCollection<SolidMaterialInfo>
{
    [Export]
    public SolidMaterialInfo DefaultMaterial;
}
