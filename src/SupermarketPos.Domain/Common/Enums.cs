namespace SupermarketPos.Domain.Common;

/// <summary>Sale unit. Piece items are counted; Kilogram items are weighed (fractional quantities).</summary>
public enum Unit
{
    Piece,
    Kilogram
}

/// <summary>Application roles. Admin can reach management screens; Cashier is limited to the register.</summary>
public enum UserRole
{
    Cashier,
    Admin
}
