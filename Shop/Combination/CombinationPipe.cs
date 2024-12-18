using Godot;
using System;

public partial class CombinationPipe : Node3DScript
{
    [NodeType]
    public Area3D Area;

    [NodeName]
    public Touchable Detach;

    public CraftingMaterial CurrentMaterial { get; private set; }

    public Action<CraftingMaterial> OnMaterialAttached { get; private set; }

    public override void _Ready()
    {
        base._Ready();

        Area.BodyEntered += b => CallDeferred(nameof(BodyEntered), b);
        Area.BodyExited += b => CallDeferred(nameof(BodyExited), b);
        Detach.OnTouched += DetachTouched;

        DisableDetaching();
    }

    private void EnableDetaching()
    {
        Detach.SetCollisionLayer(3);
    }

    private void DisableDetaching()
    {
        Detach.ClearCollisionLayerAndMask();
    }

    private void DetachTouched()
    {
        DetachMaterial();
    }

    public void DetachMaterial()
    {
        if (CurrentMaterial == null) return;

        DisableDetaching();
        CurrentMaterial.SetCollisionLayer(2, 3);
        CurrentMaterial.UnlockPosition_All();
        CurrentMaterial.UnlockRotation_All();

        CurrentMaterial.LinearVelocity = Area.GlobalBasis * Vector3.Forward * 3;

        SoundController.Instance.Play("sfx_pickup", Area.GlobalPosition);
    }

    private void BodyEntered(GodotObject obj)
    {
        var node = obj as Node3D;
        if (node == null) return;

        var mat = node.GetNodeInParents<CraftingMaterial>();
        if (mat == null) return;

        MaterialEntered(mat);
    }

    private void BodyExited(GodotObject obj)
    {
        var node = obj as Node3D;
        if (node == null) return;

        var mat = node.GetNodeInParents<CraftingMaterial>();
        if (mat == null) return;

        MaterialExited(mat);
    }

    private void MaterialEntered(CraftingMaterial mat)
    {
        if (CurrentMaterial != null) return;
        SnapMaterial(mat);
    }

    private void MaterialExited(CraftingMaterial mat)
    {
        if (mat == CurrentMaterial)
        {
            CurrentMaterial = null;
        }
    }

    private void SnapMaterial(CraftingMaterial mat)
    {
        mat.SetCollisionLayer(2, 3);
        mat.LockPosition_All();
        mat.LockRotation_All();
        mat.GlobalPosition = Area.GlobalPosition;

        CurrentMaterial = mat;
        EnableDetaching();

        SoundController.Instance.Play("sfx_pickup", Area.GlobalPosition);
    }

    public void ConsumeMaterial()
    {
        if (CurrentMaterial == null) return;

        ItemController.Instance.UntrackItem(CurrentMaterial);
        CurrentMaterial.QueueFree();
        CurrentMaterial = null;
    }
}
