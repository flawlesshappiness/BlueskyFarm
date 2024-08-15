using System;

public partial class Touchable : Interactable
{
    public event Action OnTouched;

    public void Touch()
    {
        Debug.TraceMethod();
        Debug.Indent++;

        OnTouched?.Invoke();

        Debug.Indent--;
    }
}
