public interface IPlayerGrab
{
    public bool IsGrabbing { get; }
    public bool IsRotating { get; }
    public bool CanGrab(Grabbable grabbable);
    public void Grab(Grabbable grabbable);
    public void Release();
}
