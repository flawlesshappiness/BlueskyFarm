using Godot;

public partial class ForestRoom : BasementRoom
{
    [NodeName]
    public Node3D TreeTemplate;

    [NodeName]
    public Light3D LightTemplate;

    protected override void Initialize()
    {
        base.Initialize();
        //CreateTrees();
        //CreateLights();
    }

    private void CreateTrees()
    {
        TreeTemplate.SetEnabled(false);

        var parent = TreeTemplate.GetParent();
        var y = TreeTemplate.Position.Y;
        var row_count = 6;
        var center = GlobalPosition;
        var size = SECTION_COUNT * SECTION_SIZE;
        var w = size;
        var wh = w * 0.5f;
        var x_min = Element.HasWest ? -wh : -wh + 2f;
        var x_max = Element.HasEast ? wh : wh + 2f;
        var z_min = Element.HasSouth ? -wh : -wh + 2f;
        var z_max = Element.HasNorth ? wh : wh + 2f;
        var wx = x_max - x_min;
        var wz = z_max - z_min;
        var x_per = wx / (row_count - 1);
        var z_per = wz / (row_count - 1);

        for (float z = z_min; z < z_max; z += z_per)
        {
            for (float x = x_min; x < x_max; x += x_per)
            {
                var position = center + new Vector3(x, y, z);
                var tree = TreeTemplate.Duplicate() as Node3D;
                tree.SetParent(parent);
                tree.SetEnabled(true);
                tree.GlobalPosition = position;
            }
        }
    }

    private void CreateLights()
    {
        LightTemplate.Hide();

        var parent = LightTemplate.GetParent();
        var rng = new RandomNumberGenerator();
        var size = SECTION_COUNT * SECTION_SIZE;
        var w = size - 4;
        var wh = w * 0.5f;
        var y = LightTemplate.Position.Y;
        var count = 100;
        for (int i = 0; i < count; i++)
        {
            var light = LightTemplate.Duplicate() as Light3D;
            light.SetParent(parent);
            light.Show();
            var x = rng.RandfRange(-wh, wh);
            var z = rng.RandfRange(-wh, wh);
            light.GlobalPosition = GlobalPosition + new Vector3(x, y, z);
        }
    }
}
