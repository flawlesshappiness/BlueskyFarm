using Godot;
using System;
using System.Collections;
using System.Linq;

public partial class FrogBlueprintCrafting : Node3DScript
{
    [NodeName]
    public Area3D ItemArea;

    [NodeType]
    public BlueprintDisplay Display;

    public bool HasBlueprint => Data.Game.BlueprintCraftingData != null;

    private FrogCharacter _character;

    public event Action OnBlueprintStarted;
    public event Action<string> OnBlueprintCompleted;
    public event Action OnBlueprintCancelled;
    public event Action OnMaterialReceived;

    public override void _Ready()
    {
        base._Ready();
        ItemArea.BodyEntered += go => CallDeferred(nameof(BodyEntered), go);
        Display.OnCancel += OnCancel;
        _character = this.GetNodeInParents<FrogCharacter>();

        LoadData();
    }

    private void LoadData()
    {
        var has_data = Data.Game.BlueprintCraftingData != null;
        Display.UpdateFromData(Data.Game.BlueprintCraftingData);
        Display.SetVisible(has_data);
        SetCancelEnabled(has_data);
    }

    private void BodyEntered(GodotObject go)
    {
        var node = go as Node3D;
        if (node == null) return;

        var item = node.GetNodeInParents<Item>();

        if (HasBlueprint && IsValidMaterial(item))
        {
            AddMaterial(item);
        }
        else if (!HasBlueprint && IsValidBlueprint(item))
        {
            SetBlueprint(item);
        }
    }

    private void OnCancel()
    {
        this.StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            OnBlueprintCancelled?.Invoke();

            SetCancelEnabled(false);
            yield return Display.AnimateHide();

            ItemArea.Disable();
            ThrowBlueprintToPlayer(Data.Game.BlueprintCraftingData);

            var item_datas = Data.Game.BlueprintCraftingData.Items;
            for (int i = 0; i < item_datas.Count; i++)
            {
                var t = (float)i / (item_datas.Count - 1);
                var pitch = Mathf.Lerp(1f, 1.5f, t);
                var item_data = item_datas[i];
                yield return new WaitForSeconds(0.1f);
                var item = ItemController.Instance.CreateItemFromData(item_data);
                ThrowItemToPlayer(item);
                SoundController.Instance.Play("sfx_pickup", GlobalPosition, new SoundOverride
                {
                    PitchRange = new Vector2(pitch, pitch)
                });
            }

            Data.Game.BlueprintCraftingData = null;
            yield return new WaitForSeconds(2f);

            ItemArea.Enable();
        }
    }

    private bool IsValidBlueprint(Item item)
    {
        return item != null && !string.IsNullOrEmpty(item.Data.Blueprint?.Id);
    }

    private bool IsValidMaterial(Item item)
    {
        if (!IsInstanceValid(item)) return false;
        if (item.Info == null) return false;

        var data = Data.Game.BlueprintCraftingData.Materials.FirstOrDefault(x => x.Type == item.Info.Type);
        if (data == null) return false;
        return data.Max > 0 && data.Count < data.Max;
    }

    private void SetBlueprint(Item item)
    {
        var bp_id = item.Data.Blueprint.Id;

        this.StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            SoundController.Instance.Play("sfx_throw_light", item.GlobalPosition);
            yield return item.AnimateDisappearAndQueueFree();
            yield return new WaitForSeconds(0.25f);

            SetBlueprint(bp_id);
        }
    }

    public void SetBlueprint(string bp_id)
    {
        var bp_info = BlueprintController.Instance.GetInfo(bp_id);
        Data.Game.BlueprintCraftingData = new BlueprintCraftingData
        {
            Id = bp_id,
        };

        Data.Game.BlueprintCraftingData.Materials.Add(new BlueprintCraftingMaterialData { Type = ItemType.Crop_Vegetable, Max = bp_info.VegetableCount });
        Data.Game.BlueprintCraftingData.Materials.Add(new BlueprintCraftingMaterialData { Type = ItemType.Crop_Bone, Max = bp_info.BoneCount });

        Display.UpdateFromData(Data.Game.BlueprintCraftingData);

        this.StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            OnBlueprintStarted?.Invoke();

            yield return Display.AnimateShow();
            SetCancelEnabled(true);
        }
    }

    private void AddMaterial(Item item)
    {
        if (!IsInstanceValid(item)) return;

        Data.Game.BlueprintCraftingData.Items.Add(item.Data);
        var data = Data.Game.BlueprintCraftingData.Materials.FirstOrDefault(x => x.Type == item.Info.Type);
        data.Count++;

        this.StartCoroutine(Cr, "material");
        IEnumerator Cr()
        {
            SetCancelEnabled(false);

            SoundController.Instance.Play("sfx_throw_light", item.GlobalPosition);
            yield return item.AnimateDisappearAndQueueFree();

            ValidateBlueprint();
            Display.UpdateFromData(Data.Game.BlueprintCraftingData);
            SetCancelEnabled(true);
        }
    }

    private void ValidateBlueprint()
    {
        var has_materials = Data.Game.BlueprintCraftingData.Materials.All(x => x.Count >= x.Max);
        if (!has_materials) return;

        this.StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            SetCancelEnabled(false);

            yield return Display.AnimateHide();

            CreateResult(Data.Game.BlueprintCraftingData);
            BlueprintTrigger(Data.Game.BlueprintCraftingData.Id);
            OnBlueprintCompleted?.Invoke(Data.Game.BlueprintCraftingData.Id);
            Data.Game.BlueprintCraftingData = null;

            SoundController.Instance.Play("sfx_crafting_complete");
        }
    }

    private void BlueprintTrigger(string id)
    {
        if (id == "inventory_001")
        {
            Data.Game.State_BasementInventoryPuzzle = 2;
        }
    }

    private void ThrowBlueprintToPlayer(BlueprintCraftingData data)
    {
        if (data == null) return;

        var item_bp = BlueprintController.Instance.CreateBlueprintRoll(data.Id);
        ThrowItemToPlayer(item_bp);

        SoundController.Instance.Play("sfx_throw_light", item_bp.GlobalPosition);
    }

    private void CreateResult(BlueprintCraftingData data)
    {
        if (data == null) return;

        var bp_info = BlueprintController.Instance.GetInfo(data.Id);
        if (bp_info == null) return;
        if (bp_info.ResultItemInfo == null) return;

        var position = Display.GlobalPosition;
        var item = ItemController.Instance.CreateItem(bp_info.ResultItemInfo);
        item.GlobalPosition = position;

        SoundController.Instance.Play("sfx_pickup", position);
        Particle.PlayOneShot("ps_smoke_puff_small", position);
    }

    private void ThrowItemToPlayer(Item item)
    {
        var start_position = _character.GlobalPosition.Add(y: 1);
        var direction_to_player = start_position.DirectionTo(Player.Instance.MidPosition).Normalized();
        var direction = direction_to_player.Add(y: 0.5f);
        var velocity = direction * 6f;

        item.GlobalPosition = start_position;
        item.LinearVelocity = velocity;
    }

    private void SetCancelEnabled(bool enabled)
    {
        var info = BlueprintController.Instance.GetInfo(Data.Game.BlueprintCraftingData?.Id);
        var can_cancel = info?.CanCancel ?? true;
        Display.SetCancelEnabled(enabled && can_cancel);
    }
}
