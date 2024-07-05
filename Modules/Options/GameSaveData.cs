public partial class GameSaveData
{
    public float VolumeMaster { get; set; } = 0.5f;
    public float VolumeSFX { get; set; } = 1.0f;
    public float VolumeBGM { get; set; } = 1.0f;
    public int WindowMode { get; set; } = 1;
    public int Resolution { get; set; }
    public int VSync { get; set; }
    public int FPSLimit { get; set; }
}
