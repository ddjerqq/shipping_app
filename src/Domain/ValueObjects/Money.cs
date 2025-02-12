namespace Domain.ValueObjects;

public readonly record struct Money : IComparable<Money>
{
    public Currency Currency { get; }
    public long Amount { get; }

    public Money(Currency currency, long amount)
    {
        if (Amount < 0)
            throw new InvalidOperationException("Amount cannot be negative.");

        Currency = currency;
        Amount = amount;
    }

    public Money(string currency, long amount) : this(new Currency(currency), amount)
    {
    }

    public void Deconstruct(out Currency currency, out long amount)
    {
        currency = Currency;
        amount = Amount;
    }

    public static Money operator +(Money @this, Money other) => @this.Currency == other.Currency
        ? new Money(@this.Currency, @this.Amount + other.Amount)
        : throw new InvalidOperationException("Cannot add money with different currencies. Convert the currencies first");

    public static Money operator -(Money @this, Money other) => @this.Currency == other.Currency
        ? new Money(@this.Currency, @this.Amount - other.Amount)
        : throw new InvalidOperationException("Cannot subtract money with different currencies. Convert the currencies first");

    public static bool operator >(Money @this, Money other) => @this.Currency == other.Currency
        ? @this.Amount > other.Amount
        : throw new InvalidOperationException("Cannot compare money with different currencies. Convert the currencies first");

    public static bool operator >=(Money @this, Money other) => @this.Currency == other.Currency
        ? @this.Amount >= other.Amount
        : throw new InvalidOperationException("Cannot compare money with different currencies. Convert the currencies first");

    public static bool operator <(Money @this, Money other) => @this.Currency == other.Currency
        ? @this.Amount < other.Amount
        : throw new InvalidOperationException("Cannot compare money with different currencies. Convert the currencies first");

    public static bool operator <=(Money @this, Money other) => @this.Currency == other.Currency
        ? @this.Amount <= other.Amount
        : throw new InvalidOperationException("Cannot compare money with different currencies. Convert the currencies first");

    public int CompareTo(Money other) => Currency == other.Currency
        ? Amount.CompareTo(other.Amount)
        : throw new InvalidOperationException("Cannot compare money with different currencies. Convert the currencies first");

    public static Money Parse(string value)
    {
        var parts = value.Split('-');

        return parts switch
        {
            [var currency, var amount] => new Money(new Currency(currency), long.Parse(amount)),
            _ => throw new FormatException("Invalid money format."),
        };
    }

    public string FormatedValue => $"{Currency.Symbol}{Amount / 100:F2}";

    public override string ToString() => $"{Currency}-{Amount}";
}