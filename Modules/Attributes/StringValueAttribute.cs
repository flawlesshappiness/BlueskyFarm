using System;

public class StringValueAttribute : Attribute
{
    public string Value { get; protected set; }

    public StringValueAttribute(string value)
    {
        Value = value;
    }
}

public static class StringValueAttributeExtensions
{
    public static string GetValue(this Enum e)
    {
        var type = e.GetType();
        var field_info = type.GetField(e.ToString());
        var atts = field_info.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
        return atts.Length > 0 ? atts[0].Value : null;
    }
}
