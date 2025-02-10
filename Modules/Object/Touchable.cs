using System;

public partial class Touchable : InteractableStaticBody3D
{
    public event Action OnTouched;

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
