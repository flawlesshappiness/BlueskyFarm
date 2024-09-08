using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class MaterialProcessor : Node3DScript
{
    [Export]
    public Color ColorOff;

    [Export]
    public Color ColorOn;

    [NodeName]
    public Area3D InputArea;

    [NodeName]
    public Node3D OutputPosition;

    [NodeName]
    public InteractableLever Lever;

    [NodeName]
    public Node3D Glowsticks;

    public int InputCount => Data.Game.MaterialProcessorInputs.Count;

    private List<MeshInstance3D> _glowsticks = new();

    private int _count_glowsticks_on;
    private const int COUNT_FOR_MATERIAL = 2;

    private class InputItem
    {
        public ItemInfo Info { get; set; }
        public ItemData Data { get; set; }
    }

    public override void _Ready()
    {
        base._Ready();

        InputArea.BodyEntered += BodyEntered;
        Lever.OnStateChanged += _ => StartProcessing();

        InitializeGlowsticks();
        UpdateInputCount();
    }

    private void InitializeGlowsticks()
    {
        _glowsticks = Glowsticks.GetNodesInChildren<MeshInstance3D>().ToList();
        foreach (var gs in _glowsticks)
        {
            SetGlowstickColor(gs, ColorOff);
        }
    }

    private void BodyEntered(Node3D body)
    {
        if (!IsInstanceValid(body)) return;

        var item = body.GetNodeInParents<Item>();
        if (item == null) return;

        ItemEntered(item);
    }

    private void ItemEntered(Item item)
    {
        if (!IsInstanceValid(item)) return;
        if (!IsValidInput(item)) return;

        CreateInput(item);
        ItemController.Instance.UntrackItem(item);
        item.QueueFree();
    }

    private void CreateInput(Item item)
    {
        if (InputCount == 0)
        {
            Data.Game.MaterialProcessorCurrentType = item.Info.MaterialType;
        }

        Data.Game.MaterialProcessorInputs.Add(item.Data);

        UpdateInputCount();
    }

    private bool IsValidInput(Item item)
    {
        return item.Info.CanProcessToMaterial && (InputCount == 0 || item.Info.MaterialType == Data.Game.MaterialProcessorCurrentType);
    }

    private void Clear()
    {
        Data.Game.MaterialProcessorInputs.Clear();
    }

    private void UpdateInputCount()
    {
        var count = InputCount;

        if (count == 0)
        {
            AnimateGlowsticksOff();
            return;
        }

        var t = (float)count / COUNT_FOR_MATERIAL;
        var gs_count = (int)(_glowsticks.Count * t);

        AnimateGlowsticksOn(gs_count);
    }

    private Coroutine AnimateGlowsticksOn(int count_on)
    {
        return StartCoroutine(Cr, "glowsticks");
        IEnumerator Cr()
        {
            count_on = Mathf.Clamp(count_on, 0, _glowsticks.Count);
            var i_start = Mathf.Clamp(_count_glowsticks_on, 0, _glowsticks.Count - 1);
            _count_glowsticks_on = count_on;

            for (int i = i_start; i < count_on; i++)
            {
                var gs = _glowsticks[i];
                SetGlowstickColor(gs, ColorOn);
                PlayGlowstickSFX(i);

                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private Coroutine AnimateGlowsticksOff()
    {
        return StartCoroutine(Cr, "glowsticks");
        IEnumerator Cr()
        {
            var i_start = Mathf.Clamp(_count_glowsticks_on, 0, _glowsticks.Count - 1);
            _count_glowsticks_on = 0;

            for (int i = i_start; i >= 0; i--)
            {
                var gs = _glowsticks[i];
                SetGlowstickColor(gs, ColorOff);
                PlayGlowstickSFX(i);

                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    private void PlayGlowstickSFX(int i_pitch)
    {
        var pitch = 0.9f + (0.2f * i_pitch);
        SoundController.Instance.Play("sfx_pickup", new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = GlobalPosition,
            PitchMin = pitch,
            PitchMax = pitch,
        });
    }

    private void SetGlowstickColor(MeshInstance3D mesh, Color color)
    {
        var mat = mesh.GetSurfaceOverrideMaterial(0);

        if (mat == null)
        {
            mat = mesh.Mesh.SurfaceGetMaterial(0).Duplicate() as Material;
            mesh.SetSurfaceOverrideMaterial(0, mat);
        }

        mat.Set("albedo_color", color);
        mat.Set("emission", color);
    }

    private void StartProcessing()
    {
        if (InputCount == 0)
        {
            SoundController.Instance.Play("sfx_shop_error", new SoundSettings3D
            {
                Bus = SoundBus.SFX,
                Position = GlobalPosition
            });

            return;
        }

        var count_materials = InputCount / COUNT_FOR_MATERIAL;
        var count_leftover = InputCount - count_materials * 2;
        Data.Game.MaterialProcessorInputs.Reverse();
        var leftovers = Data.Game.MaterialProcessorInputs.Take(count_leftover).ToList();
        Clear();
        UpdateInputCount();

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            for (int i = 0; i < count_materials; i++)
            {
                SpawnMaterial(Data.Game.MaterialProcessorCurrentType);
                yield return new WaitForSeconds(0.2f);
            }

            foreach (var input in leftovers)
            {
                SpawnInputItem(input);
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    private void SpawnMaterial(CraftingMaterialType type)
    {
        var path = GetMaterialPath(type);
        var item = ItemController.Instance.CreateItem(path);
        SpawnOutput(item);
    }

    private string GetMaterialPath(CraftingMaterialType type) => type switch
    {
        CraftingMaterialType.Plant => ItemController.Instance.Collection.PlantMaterial,
        CraftingMaterialType.Metal => ItemController.Instance.Collection.MetalMaterial,
        CraftingMaterialType.Wood => ItemController.Instance.Collection.WoodMaterial,
        CraftingMaterialType.Glow => ItemController.Instance.Collection.GlowMaterial,
        CraftingMaterialType.Rock => ItemController.Instance.Collection.RockMaterial,
        CraftingMaterialType.Crystal => ItemController.Instance.Collection.CrystalMaterial,
        _ => ItemController.Instance.Collection.PlantMaterial
    };

    private void SpawnInputItem(ItemData data)
    {
        var item = ItemController.Instance.CreateItemFromData(data);
        SpawnOutput(item);
    }

    private void SpawnOutput(Item item)
    {
        item.GlobalPosition = OutputPosition.GlobalPosition;
        item.RigidBody.LinearVelocity = OutputPosition.GlobalBasis * Vector3.Forward * 3;

        SoundController.Instance.Play("sfx_pickup", new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = OutputPosition.GlobalPosition,
            PitchMin = 0.9f,
            PitchMax = 1.0f,
        });
    }
}
