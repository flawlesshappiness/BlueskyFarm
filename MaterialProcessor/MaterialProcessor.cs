using Godot;
using System.Collections;
using System.Linq;

public partial class MaterialProcessor : Node3DScript
{
    [NodeName]
    public Area3D InputArea;

    [NodeName]
    public Node3D OutputPosition;

    [NodeName]
    public Label3D InputCountLabel;

    [NodeName]
    public Touchable ProcessButton;

    public int InputCount => Data.Game.MaterialProcessorInputs.Count;

    private class InputItem
    {
        public ItemInfo Info { get; set; }
        public ItemData Data { get; set; }
    }

    public override void _Ready()
    {
        base._Ready();

        InputArea.BodyEntered += BodyEntered;
        ProcessButton.OnTouched += StartProcessing;

        UpdateInputCount();
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

        SoundController.Instance.Play("sfx_pickup", new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = InputArea.GlobalPosition,
            PitchMin = 0.9f,
            PitchMax = 1.0f,
        });
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
        var type = count > 0 ? Data.Game.MaterialProcessorCurrentType.ToString() : "";
        InputCountLabel.Text = $"{count} {type}";
    }

    private void StartProcessing()
    {
        var count_materials = Data.Game.MaterialProcessorInputs.Count / 2;
        var count_leftover = Data.Game.MaterialProcessorInputs.Count - count_materials * 2;
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
