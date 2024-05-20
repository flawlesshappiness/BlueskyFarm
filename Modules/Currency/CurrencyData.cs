using System;
using System.Text.Json.Serialization;

public class CurrencyData
{
    public CurrencyType Type { get; set; }
    public int Value { get; set; }

    [JsonIgnore]
    public Action<int> OnValueChanged;
}
