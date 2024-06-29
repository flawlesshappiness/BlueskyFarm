public partial class CurrencyType
{
    public static readonly CurrencyType Money = new CurrencyType(nameof(Money));

    public string Id { get; private set; }

    public CurrencyType(string id)
    {
        Id = id;
    }

    public override string ToString()
    {
        return Id;
    }

    public override bool Equals(object obj)
    {
        if (Id == null) return false;
        return Id == (obj as CurrencyType)?.Id;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
