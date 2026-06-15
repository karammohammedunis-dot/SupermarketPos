using SupermarketPos.Domain.Common;

namespace SupermarketPos.Domain.Entities;

public sealed class Category
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Category() { } // EF

    public Category(string name)
    {
        Rename(name);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name cannot be empty.");
        Name = name.Trim();
    }
}
