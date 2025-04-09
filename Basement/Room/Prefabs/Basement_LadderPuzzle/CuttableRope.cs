using System;

public partial class CuttableRope : Touchable, ICuttable
{
    public event Action OnCut;

    public void Cut()
    {
        OnCut?.Invoke();
    }
}
