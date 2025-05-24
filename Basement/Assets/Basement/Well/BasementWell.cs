using Godot;
using System;
using System.Collections;

public partial class BasementWell : Node3DScript
{
    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public Node3D Handle;

    [Export]
    public Touchable HandleTouchable;

    [Export]
    public Node3D RopeEndPosition;

    [Export]
    public Touchable RopeEndTouchable;

    public bool CanEnableRopeEndTouchable { get; set; }
    public bool IsRaised { get; private set; }

    public event Action OnRaise;
    public event Action OnLower;
    public event Action OnRaised;
    public event Action OnLowered;
    public event Action OnRopeEndTouched;

    private bool raised;

    public override void _Ready()
    {
        base._Ready();
        HandleTouchable.OnTouched += HandleTouchable_Touched;
        RopeEndTouchable.OnTouched += RopeEndTouchable_Touched;
    }

    private void RopeEndTouchable_Touched()
    {
        RopeEndTouchable.Disable();
        OnRopeEndTouched?.Invoke();
    }

    private void HandleTouchable_Touched()
    {
        ToggleRaised();
    }

    public void ToggleRaised()
    {
        HandleTouchable.Disable();
        RopeEndTouchable.Disable();

        if (raised)
        {
            AnimateLower();
        }
        else
        {
            AnimateRaise();
        }

        raised = !raised;
    }

    public void AnimateRaise()
    {
        this.StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            OnRaise?.Invoke();
            yield return AnimationPlayer.PlayAndWaitForAnimation("raise");
            TryEnableRopeEndTouchable();
            HandleTouchable.Enable();
            OnRaised?.Invoke();

            IsRaised = true;
        }
    }

    public void AnimateLower()
    {
        this.StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            IsRaised = false;

            OnLower?.Invoke();
            yield return AnimationPlayer.PlayAndWaitForAnimation("lower");
            HandleTouchable.Enable();
            OnLowered?.Invoke();
        }
    }

    public void AttachObjectToRope(Node3D node)
    {
        node.SetParent(RopeEndPosition);
        node.Position = Vector3.Zero;
        node.Rotation = Vector3.Zero;
    }

    public void SetHandleEnabled(bool enabled)
    {
        Handle.SetEnabled(enabled);
        HandleTouchable.SetEnabled(enabled);
    }

    public void TryEnableRopeEndTouchable()
    {
        if (!CanEnableRopeEndTouchable) return;
        RopeEndTouchable.Enable();
    }

    public void DisableRopeEndTouchable()
    {
        RopeEndTouchable.Disable();
    }
}