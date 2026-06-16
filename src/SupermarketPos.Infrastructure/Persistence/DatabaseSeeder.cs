using Microsoft.EntityFrameworkCore;
using SupermarketPos.Application.Abstractions;
using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Entities;

namespace SupermarketPos.Infrastructure.Persistence;

/// <summary>
/// First-run seed: store settings, a couple of accounts, categories and ~15 realistic products
/// with a mix of KDV rates (1% / 10% / 20%) and both Piece and Kilogram units.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(PosDbContext db, IPasswordHasher hasher, CancellationToken ct = default)
    {
        if (!await db.Settings.AnyAsync(ct))
        {
            db.Settings.Add(new AppSettings(
                storeName: "FreshMarket Süpermarket",
                storeAddress: "Atatürk Cad. No:42, İstanbul",
                language: "tr",
                lowStockThreshold: 5m,
                vatRates: new[] { 0.01m, 0.10m, 0.20m }));
        }

        if (!await db.Users.AnyAsync(ct))
        {
            db.Users.Add(new User("admin", hasher.Hash("admin"), "Yönetici", UserRole.Admin));
            db.Users.Add(new User("kasa", hasher.Hash("1234"), "Kasiyer", UserRole.Cashier));
        }

        if (await db.Products.AnyAsync(ct))
        {
            await db.SaveChangesAsync(ct);
            return;
        }

        var beverages = new Category("İçecekler");
        var dairy = new Category("Süt Ürünleri");
        var snacks = new Category("Atıştırmalık");
        var produce = new Category("Manav");
        var bakery = new Category("Fırın");
        var cleaning = new Category("Temizlik");
        db.Categories.AddRange(beverages, dairy, snacks, produce, bakery, cleaning);
        await db.SaveChangesAsync(ct);

        var products = new[]
        {
            // Beverages
            new Product("8690000000017", "Su 0.5L",            2.50m, 0.01m, Unit.Piece, 120, beverages.Id),
            new Product("8690000000024", "Kola 1L",            34.90m, 0.10m, Unit.Piece, 60,  beverages.Id),
            new Product("8690000000031", "Portakal Suyu 1L",   29.75m, 0.01m, Unit.Piece, 40,  beverages.Id),
            new Product("8690000000048", "Türk Kahvesi 100g",  52.00m, 0.10m, Unit.Piece, 25,  beverages.Id),
            // Dairy
            new Product("8690000000055", "Süt 1L",             27.50m, 0.01m, Unit.Piece, 50,  dairy.Id),
            new Product("8690000000062", "Yoğurt 1kg",         44.90m, 0.01m, Unit.Piece, 30,  dairy.Id),
            new Product("8690000000079", "Beyaz Peynir 500g",  89.90m, 0.01m, Unit.Piece, 18,  dairy.Id),
            new Product("8690000000086", "Tereyağı 250g",      74.50m, 0.10m, Unit.Piece, 4,   dairy.Id),
            // Snacks
            new Product("8690000000093", "Çikolata 80g",       19.90m, 0.20m, Unit.Piece, 80,  snacks.Id),
            new Product("8690000000109", "Cips 150g",          27.50m, 0.20m, Unit.Piece, 45,  snacks.Id),
            new Product("8690000000116", "Bisküvi 200g",       14.75m, 0.10m, Unit.Piece, 3,   snacks.Id),
            // Produce (weighed) — variable-measure EAN-13: "20" + 5-digit PLU + "00000" + check.
            // A scale prints the same PLU with the real weight in grams (e.g. 2000001012505 = 1.250 kg Domates).
            new Product("2000001000007", "Domates",            21.90m, 0.01m, Unit.Kilogram, 35, produce.Id),
            new Product("2000002000006", "Elma",               18.50m, 0.01m, Unit.Kilogram, 50, produce.Id),
            new Product("2000003000005", "Muz",                39.90m, 0.01m, Unit.Kilogram, 22, produce.Id),
            // Bakery
            new Product("8690000000123", "Ekmek",               8.50m, 0.01m, Unit.Piece, 90,  bakery.Id),
            // Cleaning
            new Product("8690000000130", "Bulaşık Deterjanı 1L", 64.90m, 0.20m, Unit.Piece, 2, cleaning.Id),
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync(ct);
    }
}
