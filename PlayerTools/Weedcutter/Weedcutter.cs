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
        Area.BodyEntered += body => CallDeferred(nameof(OnBodyEntered), body);
        Area.BodyExited += body => CallDeferred(nameof(OnBodyExited), body);
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
            if (body is Node node && node != null)
            {
                var cuttable = node.GetNodeInParents<ICuttable>();
                if (cuttable == null) continue;

                Cut(cuttable);
            }
        }

        Clear();
    }

    private void Cut(ICuttable cuttable)
    {
        cuttable.Cut();
    }

    public void Clear()
    {
        _bodies.Clear();
    }

    private void OnBodyEntered(GodotObject g)
    {
        _bodies.Add(g);
    }

    private void OnBodyExited(GodotObject g)
    {
        _bodies.Remove(g);
    }
}
