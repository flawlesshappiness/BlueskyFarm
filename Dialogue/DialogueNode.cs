using System.Collections.Generic;

public class DialogueNode
{
    public string id { get; set; }
    public string next { get; set; }
    public string animation { get; set; }
    public string character { get; set; }
    public IEnumerable<string> triggers { get; set; }
    public IEnumerable<DialogueFlag> flags { get; set; }
}
