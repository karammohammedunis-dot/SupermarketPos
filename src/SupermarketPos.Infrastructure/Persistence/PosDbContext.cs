using Microsoft.EntityFrameworkCore;
using SupermarketPos.Domain.Entities;

namespace SupermarketPos.Infrastructure.Persistence;

public sealed class PosDbContext : DbContext
{
    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleLine> SaleLines => Set<SaleLine>();
    public DbSet<AppSettings> Settings => Set<AppSettings>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Money stays decimal; the SQLite provider stores decimal as TEXT (never REAL/float),
        // preserving exact values. Numeric comparisons over money are done in memory, not SQL.

        b.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });

        b.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Barcode).IsRequired().HasMaxLength(64);
            e.HasIndex(p => p.Barcode).IsUnique();
            e.Property(p => p.Name).IsRequired().HasMaxLength(200);
            e.Property(p => p.UnitPrice).HasColumnType("TEXT");
            e.Property(p => p.VatRate).HasColumnType("TEXT");
            e.Property(p => p.StockQuantity).HasColumnType("TEXT");
            e.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).IsRequired().HasMaxLength(64);
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
            e.Property(u => u.Role).HasConversion<string>();
        });

        b.Entity<Sale>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.ReceiptNo).IsRequired().HasMaxLength(32);
            e.HasIndex(s => s.ReceiptNo).IsUnique();
            e.Property(s => s.CashierName).HasMaxLength(100);
            e.Property(s => s.GrossTotal).HasColumnType("TEXT");
            e.Property(s => s.NetTotal).HasColumnType("TEXT");
            e.Property(s => s.VatTotal).HasColumnType("TEXT");
            e.Property(s => s.PaymentMethod).IsRequired().HasMaxLength(8);
            e.Property(s => s.AmountPaid).HasColumnType("TEXT");
            e.Property(s => s.CashTendered).HasColumnType("TEXT");
            e.Property(s => s.ChangeGiven).HasColumnType("TEXT");
            e.Ignore(s => s.VatBreakdown); // computed from lines
            e.HasMany(s => s.Lines)
                .WithOne()
                .HasForeignKey(l => l.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(s => s.Lines)
                .HasField("_lines")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<SaleLine>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Barcode).HasMaxLength(64);
            e.Property(l => l.Name).IsRequired().HasMaxLength(200);
            e.Property(l => l.UnitPrice).HasColumnType("TEXT");
            e.Property(l => l.VatRate).HasColumnType("TEXT");
            e.Property(l => l.Quantity).HasColumnType("TEXT");
            e.Property(l => l.LineGross).HasColumnType("TEXT");
            e.Property(l => l.DiscountDescription).HasMaxLength(64);
            e.Ignore(l => l.NetAmount);
            e.Ignore(l => l.VatAmount);
        });

        b.Entity<AppSettings>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.StoreName).IsRequired().HasMaxLength(150);
            e.Property(s => s.StoreAddress).HasMaxLength(300);
            e.Property(s => s.Language).HasMaxLength(5);
            e.Property(s => s.LowStockThreshold).HasColumnType("TEXT");
            e.Property(s => s.VatRatesCsv).IsRequired().HasMaxLength(100);
            e.Ignore(s => s.VatRates);
        });
    }
}
