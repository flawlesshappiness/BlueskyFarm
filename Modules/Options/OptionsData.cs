using System.Collections.Generic;

public partial class OptionsData : SaveData
{
    public int SelectedGameProfile { get; set; } = 1;
    public float VolumeMaster { get; set; } = 0.5f;
    public float VolumeSFX { get; set; } = 1.0f;
    public float VolumeBGM { get; set; } = 1.0f;
    public int WindowMode { get; set; } = 1;
    public int Resolution { get; set; } = 4;
    public int VSync { get; set; } = 2;
    public int FPSLimit { get; set; } = 4;
    public float Brightness { get; set; } = 1.0f;
    public List<InputEventKeyData> KeyOverrides { get; set; } = new();
    public List<InputEventMouseButtonData> MouseButtonOverrides { get; set; } = new();
}
