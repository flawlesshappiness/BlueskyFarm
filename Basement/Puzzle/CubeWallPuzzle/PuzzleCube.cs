using Godot;
using System.Collections.Generic;

public partial class PuzzleCube : Item
{
    public class Alignment
    {
        public Vector3 Direction { get; set; }
        public Vector3 Rotation { get; set; }
        public Type Type { get; set; }
    }

    public enum Type
    {
        Diagonal, // north
        Arrow, // east
        F, // south
        Roots, // west
        Leaf, // top
        Tree, // bottom
    }

    public enum Color
    {
        Red,
        Green,
        Blue,
        Yellow,

        Disabled,
    }

    public static List<Alignment> Alignments = new()
    {
        new Alignment {
            Direction = Vector3.Back,
            Type = Type.Diagonal,
            Rotation = new Vector3(0, 0, 0)
        },

        new Alignment {
            Direction = Vector3.Right,
            Type = Type.Arrow,
            Rotation = new Vector3(0, -90, 0)
        },

        new Alignment {
            Direction = Vector3.Forward,
            Type = Type.F,
            Rotation = new Vector3(0, 180, 0)
        },

        new Alignment {
            Direction = Vector3.Left,
            Type = Type.Roots,
            Rotation = new Vector3(0, 90, 0)
        },

        new Alignment {
            Direction = Vector3.Up,
            Type = Type.Tree,
            Rotation = new Vector3(0, 90, 90)
        },

        new Alignment {
            Direction = Vector3.Down,
            Type = Type.Leaf,
            Rotation = new Vector3(0, 90, -90)
        },
    };

    [NodeType]
    public PuzzleCubeDisplay Display;

    [NodeType]
    public OmniLight3D Light;

    public override void LoadFromData()
    {
        base.LoadFromData();
        SetColor(Data.Uses);
    }

    private void SetRandomColor()
    {
        var rng = new RandomNumberGenerator();
        var i = rng.RandiRange(0, 3);
        SetColor(i);
    }

    public override void Use()
    {
        base.Use();

        var i = ((int)Display.CurrentColor + 1) % 4;
        SetColor(i);

        SoundController.Instance.Play("sfx_stone_impact", GlobalPosition, new SoundOverride
        {
            Volume = -30
        });
    }

    private void SetColor(int i)
    {
        Data.Uses = i;
        var color = (Color)Data.Uses;
        Display.SetColor(color);

        var mat = Display.GetMaterial(color);
        Light.LightColor = mat.Get("emission").AsColor();
    }
}
