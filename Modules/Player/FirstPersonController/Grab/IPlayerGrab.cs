public interface IPlayerGrab
{
    public bool IsGrabbing { get; }
    public bool IsRotating { get; }
    public void Grab(IGrabbable grabbable);
    public void Release();
}
