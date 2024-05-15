using System;

public partial class NodeTypeAttribute : Attribute
{
    public Type Type { get; protected set; }

    public NodeTypeAttribute(Type type)
    {
        Type = type;
    }
}
