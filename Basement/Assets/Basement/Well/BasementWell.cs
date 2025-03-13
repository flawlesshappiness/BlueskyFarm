using Godot;
using System;

public partial class BasementWell : Node3DScript
{
    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeName]
    public Node3D Handle;

    [NodeName]
    public Touchable HandleTouchable;

    [NodeName]
    public Node3D RopeEndPosition;

    public event Action OnRaise;
    public event Action OnRaiseFinished;
    public event Action OnLower;

    private bool _raised;

    public override void _Ready()
    {
        base._Ready();
        AnimationPlayer.AnimationFinished += AnimationFinished;
        HandleTouchable.OnTouched += HandleTouchable_Touched;
    }

    private void AnimationFinished(StringName animation)
    {
        HandleTouchable.SetEnabled(true);

        if (animation == "raise")
        {
            OnRaiseFinished?.Invoke();
        }
    }

    private void HandleTouchable_Touched()
    {
        ToggleRaised();
    }

    public void ToggleRaised()
    {
        if (_raised)
        {
            TryLower();
        }
        else
        {
            TryRaise();
        }
    }

    public void TryRaise()
    {
        if (_raised) return;
        _raised = true;
        HandleTouchable.SetEnabled(false);
        AnimateRaise();
    }

    public void TryLower()
    {
        if (!_raised) return;
        _raised = false;
        HandleTouchable.SetEnabled(false);
        AnimateLower();
    }

    public void AnimateRaise()
    {
        AnimationPlayer.Play("raise");
        OnRaise?.Invoke();
    }

    public void AnimateLower()
    {
        AnimationPlayer.Play("lower");
        OnLower?.Invoke();
    }

    public void AttachObjectToRope(Node3D node)
    {
        node.SetParent(RopeEndPosition);
        node.Position = Vector3.Zero;
        node.Rotation = Vector3.Zero;
    }
}