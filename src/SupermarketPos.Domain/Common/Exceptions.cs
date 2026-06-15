namespace SupermarketPos.Domain.Common;

/// <summary>Base type for all domain rule violations. Carries a user-presentable message.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

/// <summary>Thrown when a stock decrease would drive a product's quantity below zero.</summary>
public sealed class InsufficientStockException : DomainException
{
    public string ProductName { get; }
    public decimal Available { get; }
    public decimal Requested { get; }

    public InsufficientStockException(string productName, decimal available, decimal requested)
        : base($"Insufficient stock for '{productName}': available {available}, requested {requested}.")
    {
        ProductName = productName;
        Available = available;
        Requested = requested;
    }
}

/// <summary>Thrown for authentication / authorization failures.</summary>
public sealed class AuthException : DomainException
{
    public AuthException(string message) : base(message) { }
}
