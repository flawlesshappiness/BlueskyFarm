using System.Collections.Generic;

public class DialogueNodeCollection
{
    public Dictionary<string, DialogueNode> Nodes = new();

    public DialogueNodeCollection(IEnumerable<DialogueNode> nodes)
    {
        foreach (var node in nodes)
        {
            Nodes.Add(node.name, node);
        }
    }
}
