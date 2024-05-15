using System;

public class NodePathAttribute : Attribute
{
    public string Value { get; protected set; }

    public NodePathAttribute(string value)
    {
        Value = value;
    }
}
