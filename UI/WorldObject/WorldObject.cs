using Godot;
using System.Collections;

public partial class WorldObject : ControlScript
{
    [NodeName]
    public Node3D Origin;

    [NodeName]
    public Node3D Spin;

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
        var item = ItemController.Instance.CreateItem(info, parent: Origin, track_item: false);
        item.RigidBody.ProcessMode = ProcessModeEnum.Disabled;
        SetObject(item);
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
                    Spin.GlobalRotationDegrees = new Vector3(0, y, 0);
                });
            }
        }
    }

    public void StopAnimateSpin()
    {
        Spin.GlobalRotationDegrees = new Vector3(0, 0, 0);
        Coroutine.Stop("spin" + GetInstanceId());
    }
}
