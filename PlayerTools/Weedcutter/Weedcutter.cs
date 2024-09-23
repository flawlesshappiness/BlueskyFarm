using Godot;
using System.Collections;
using System.Collections.Generic;

public partial class Weedcutter : Item
{
    [NodeType]
    public AnimationPlayer Animation;

    [NodeType]
    public Area3D Area;

    private bool _cutting;
    private List<GodotObject> _bodies = new();

    public override void _Ready()
    {
        base._Ready();
        Area.BodyEntered += body => CallDeferred(nameof(BodyEntered), body);
        Area.BodyExited += body => CallDeferred(nameof(BodyExited), body);
    }

    public override void Use()
    {
        base.Use();
        StartCut();
    }

    public void StartCut()
    {
        if (_cutting) return;
        _cutting = true;
        Animation.Play("cut");

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(0.5f);
            CutAll();
            yield return new WaitForSeconds(0.7f);
            _cutting = false;
        }
    }

    private void CutAll()
    {
        foreach (var body in _bodies)
        {
            if (!IsInstanceValid(body)) continue;

            var node = body as Node3D;
            if (node == null) continue;

            var weed = node.GetNodeInParents<Weed>();
            if (weed == null) continue;

            Cut(weed);
        }

        Clear();
    }

    private void Cut(Weed weed)
    {
        weed.Cut();
    }

    public void Clear()
    {
        _bodies.Clear();
    }

    private void BodyEntered(GodotObject g)
    {
        _bodies.Add(g);
    }

    private void BodyExited(GodotObject g)
    {
        _bodies.Remove(g);
    }
}
