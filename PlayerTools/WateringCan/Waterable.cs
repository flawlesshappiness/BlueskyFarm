using System;

public partial class Waterable : Node3DScript
{
    public event Action OnWatered;

    public void Water()
    {
        OnWatered?.Invoke();
    }
}
