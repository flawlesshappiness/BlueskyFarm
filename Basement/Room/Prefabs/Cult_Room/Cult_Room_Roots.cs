using Godot;
using System.Linq;

public partial class Cult_Room_Roots : Node3DScript
{
    protected override void Initialize()
    {
        base.Initialize();
        InitializeRoots();
    }

    private void InitializeRoots()
    {
        var children = this.GetNodesInChildren<Node3D>();
        children.ForEach(x => x.Hide());
        var count = children.Count();
        var t = GetRootTValue();
        var curve = Curves.EaseInOutSine;
        var t_curve = curve.Evaluate(t);
        var take_count = (int)Mathf.Lerp(count, 0, t);
        var take = children.TakeRandom(take_count);
        take.ForEach(x => x.Show());
        //Debug.Log(take_count);
    }

    private float GetRootTValue()
    {
        var min = BasementRoom.ROOM_SIZE;
        var max = BasementRoom.ROOM_SIZE * 5;
        var dist = GetDistanceToTreeRoom();
        var t = (dist - min) / (max - min);
        //Debug.Log(t);
        return t;
    }

    private float GetDistanceToTreeRoom()
    {
        var info = BasementRoomController.Instance.Collection.GetResource("Cult_Tree");
        var tree_room_element = BasementController.Instance.CurrentBasement.Grid.Elements.FirstOrDefault(x => x.Info == info);
        if (tree_room_element == null) return 0;

        var tree_room_position = tree_room_element.Room.GlobalPosition;
        var dist = GlobalPosition.DistanceTo(tree_room_position);
        //Debug.Log($"{GlobalPosition} > {tree_room_position} = {dist}");
        return dist;
    }
}
