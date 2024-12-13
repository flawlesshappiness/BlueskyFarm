using Godot;

public partial class PuzzleCubeDisplay : Node3DScript
{
    [Export]
    public Material RedMaterial;

    [Export]
    public Material GreenMaterial;

    [Export]
    public Material BlueMaterial;

    [Export]
    public Material YellowMaterial;

    [NodeName]
    public MeshInstance3D Cube;

    public PuzzleCube.Color CurrentColor { get; set; }

    public void SetColor(PuzzleCube.Color color)
    {
        var mat = GetMaterial(color);
        Cube.SetSurfaceOverrideMaterial(1, mat);
        CurrentColor = color;
    }

    public Material GetMaterial(PuzzleCube.Color color)
    {
        return color switch
        {
            PuzzleCube.Color.Red => RedMaterial,
            PuzzleCube.Color.Green => GreenMaterial,
            PuzzleCube.Color.Blue => BlueMaterial,
            PuzzleCube.Color.Yellow => YellowMaterial,
            _ => RedMaterial
        };
    }
}
