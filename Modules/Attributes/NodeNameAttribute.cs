using System;

/// <summary>
/// Finds the first child instance with this name, and assigns it to the decorated field.
/// If no value is given, the name of the field is used instead.
/// </summary>
public class NodeNameAttribute : Attribute
{
    public string Value { get; protected set; }

    public NodeNameAttribute(string value)
    {
        Value = value;
    }

    public NodeNameAttribute()
    {
        Value = null;
    }
}
