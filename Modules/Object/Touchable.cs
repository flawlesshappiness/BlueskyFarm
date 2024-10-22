using System;

public partial class Touchable : Interactable
{
    public event Action OnTouched;

    protected override void _ReadyPlayer()
    {
        base._ReadyPlayer();

        if (!OverrideInitialCollisionMode)
        {
            SetCollision_Interactable();
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
