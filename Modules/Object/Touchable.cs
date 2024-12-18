using Godot;
using System;

public partial class Touchable : InteractableStaticBody3D
{
    [Export]
    public bool OverrideCollisionMode;

    public event Action OnTouched;

    public override void _Ready()
    {
        base._Ready();

        if (!OverrideCollisionMode)
        {
            SetCollisionLayer(3);
            SetCollisionMask();
        }
    }

    public void Touch()
    {
        Debug.TraceMethod();
        Debug.Indent++;

        Touched();

        Debug.Indent--;
    }

    protected virtual void Touched()
    {
        OnTouched?.Invoke();
    }

    public virtual bool HandleCursor()
    {
        return false;
    }
}
