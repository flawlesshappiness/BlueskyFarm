public class WaitForSeconds : CustomYieldInstruction
{
    public double StartTime { get; private set; }
    public double EndTime { get; private set; }
    public double CurrentTime => GameTime.Time;
    public override bool KeepWaiting => CurrentTime < EndTime;

    public WaitForSeconds(double seconds)
    {
        StartTime = CurrentTime;
        EndTime = StartTime + seconds;
    }

    public override string ToString()
    {
        return base.ToString() + $"({CurrentTime - StartTime} / {EndTime - StartTime})";
    }
}
