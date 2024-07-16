using Godot;

public partial class PsMushroomSmoke : Node3DScript
{
    [NodeName]
    public GpuParticles3D SmokeUp;

    [NodeName]
    public GpuParticles3D SmokeOut;

    [NodeName]
    public GpuParticles3D SmokePuff;

    [NodeName]
    public GpuParticles3D SmokeIdle;

    public void PlayPuff()
    {
        SmokePuff.Emitting = true;
    }

    public void PlayIdle()
    {
        SmokeIdle.Emitting = true;
    }

    public void StopIdle()
    {
        SmokeIdle.Emitting = false;
    }
}
