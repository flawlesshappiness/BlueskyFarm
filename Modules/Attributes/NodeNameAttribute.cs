using System;

public class NodeNameAttribute : Attribute
{
    public string Value { get; protected set; }

    public NodeNameAttribute(string value)
    {
        Value = value;
    }
}
