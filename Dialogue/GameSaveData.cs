using System.Collections.Generic;

public partial class GameSaveData
{
    public List<DialogueFlagBool> DialogFlags_Bool { get; set; } = new();
    public List<DialogueFlagInt> DialogFlags_Int { get; set; } = new();
}
