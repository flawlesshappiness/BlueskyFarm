using System.Collections.Generic;

public partial class GameSaveData
{
    public string CurrentScene { get; set; }
    public List<SceneData> Scenes { get; set; } = new();
}
