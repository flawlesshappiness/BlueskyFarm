using Godot;
using System.Collections;

public partial class WorldObject : ControlScript
{
    [NodeName]
    public Node3D Origin;

    [NodeName]
    public Node3D Spin;

    [NodeName]
    public Node3D RotationOffset;

    [NodeType]
    public Camera3D Camera;

    [NodeType]
    public SubViewportContainer SubViewportContainer;

    private Node3D _current;

    public void Clear()
    {
        if (_current == null) return;

        _current.QueueFree();
        _current = null;
    }

    public void SetObject(Node3D obj)
    {
        Clear();

        _current = obj;
        _current.SetParent(Origin);
        _current.Transform = Origin.GlobalTransform;
    }

    public void LoadItem(ItemInfo info)
    {
        var item = ItemController.Instance.CreateItem(info, new CreateItemSettings
        {
            Parent = Origin,
            Tracked = false,
        });

        item.RigidBody.ProcessMode = ProcessModeEnum.Disabled;
        SetObject(item);
        UpdateCameraFromItemInfo(info);
        UpdateRotationOffsetFromItemInfo(info);
    }

    private void UpdateCameraFromItemInfo(ItemInfo info)
    {
        Camera.Position = new Vector3(0, 0, info.InventoryItemCameraDistance);
    }

    private void UpdateRotationOffsetFromItemInfo(ItemInfo info)
    {
        RotationOffset.RotationDegrees = new Vector3(info.InventoryItemRotationOffset, 0f, 0f);
    }

    public void StartAnimateSpin()
    {
        StartCoroutine(Cr, "spin");
        IEnumerator Cr()
        {
            var start = 0;
            var end = 360f;
            var duration = 2f;

            while (true)
            {
                yield return LerpEnumerator.Lerp01(duration, f =>
                {
                    var y = Mathf.Lerp(start, end, f);
                    Spin.RotationDegrees = new Vector3(0, y, 0);
                });
            }
        }
    }

    public void StopAnimateSpin()
    {
        Spin.RotationDegrees = new Vector3(0, 0, 0);
        Coroutine.Stop("spin" + GetInstanceId());
    }
}
