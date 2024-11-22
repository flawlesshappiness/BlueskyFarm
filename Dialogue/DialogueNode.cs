using System.Collections.Generic;

public class DialogueNode
{
    public string id { get; set; }
    public string next { get; set; }
    public string animation { get; set; }
    public IEnumerable<string> triggers { get; set; }
    public IEnumerable<DialogueFlagBool> flags_b { get; set; }
    public IEnumerable<DialogueFlagInt> flags_i { get; set; }
}
