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
        var breakfast = new Category("Kahvaltılık");
        var pantry = new Category("Bakliyat & Makarna");
        var frozen = new Category("Dondurulmuş");
        var care = new Category("Kişisel Bakım");
        var butcher = new Category("Kasap");
        db.Categories.AddRange(beverages, dairy, snacks, produce, bakery, cleaning, breakfast, pantry, frozen, care, butcher);
        await db.SaveChangesAsync(ct);

        var seq = 1;
        Product P(string name, decimal price, decimal vat, int catId, decimal stock) =>
            new(Ean13("869" + (seq++).ToString("D9")), name, price, vat, Unit.Piece, stock, catId);
        // Weighed item with an in-store variable-measure barcode: "20" + 5-digit PLU + "00000" + check.
        Product W(string plu, string name, decimal price, decimal vat, int catId, decimal stock) =>
            new(Ean13("20" + plu + "00000"), name, price, vat, Unit.Kilogram, stock, catId);

        var products = new[]
        {
            // İçecekler
            P("Su 0.5L", 2.50m, 0.01m, beverages.Id, 120),
            P("Su 5L", 12.90m, 0.01m, beverages.Id, 40),
            P("Kola 1L", 34.90m, 0.10m, beverages.Id, 60),
            P("Kola 330ml", 18.50m, 0.10m, beverages.Id, 90),
            P("Portakal Suyu 1L", 29.75m, 0.01m, beverages.Id, 40),
            P("Ayran 300ml", 9.50m, 0.01m, beverages.Id, 70),
            P("Maden Suyu 200ml", 7.50m, 0.10m, beverages.Id, 100),
            P("Türk Kahvesi 100g", 52.00m, 0.10m, beverages.Id, 25),
            P("Çay 1kg", 145.00m, 0.01m, beverages.Id, 18),
            P("Enerji İçeceği 250ml", 32.00m, 0.20m, beverages.Id, 3),

            // Süt Ürünleri
            P("Süt 1L", 27.50m, 0.01m, dairy.Id, 50),
            P("Yoğurt 1kg", 44.90m, 0.01m, dairy.Id, 30),
            P("Beyaz Peynir 500g", 89.90m, 0.01m, dairy.Id, 18),
            P("Kaşar Peyniri 400g", 119.00m, 0.01m, dairy.Id, 14),
            P("Tereyağı 250g", 74.50m, 0.10m, dairy.Id, 4),
            P("Yumurta 10'lu", 54.90m, 0.01m, dairy.Id, 40),
            P("Krema 200ml", 24.50m, 0.10m, dairy.Id, 22),

            // Atıştırmalık
            P("Çikolata 80g", 19.90m, 0.20m, snacks.Id, 80),
            P("Cips 150g", 27.50m, 0.20m, snacks.Id, 45),
            P("Bisküvi 200g", 14.75m, 0.10m, snacks.Id, 3),
            P("Kraker 100g", 12.50m, 0.10m, snacks.Id, 60),
            P("Karışık Kuruyemiş 250g", 89.90m, 0.10m, snacks.Id, 16),
            P("Gofret 40g", 7.50m, 0.20m, snacks.Id, 120),

            // Fırın
            P("Ekmek", 8.50m, 0.01m, bakery.Id, 90),
            P("Tam Buğday Ekmek", 14.00m, 0.01m, bakery.Id, 35),
            P("Simit", 7.50m, 0.01m, bakery.Id, 50),
            P("Poğaça", 12.00m, 0.10m, bakery.Id, 40),
            P("Kek 250g", 34.90m, 0.10m, bakery.Id, 20),

            // Temizlik
            P("Bulaşık Deterjanı 1L", 64.90m, 0.20m, cleaning.Id, 2),
            P("Çamaşır Deterjanı 2L", 159.00m, 0.20m, cleaning.Id, 12),
            P("Yüzey Temizleyici 750ml", 49.90m, 0.20m, cleaning.Id, 18),
            P("Çöp Poşeti 10'lu", 29.90m, 0.20m, cleaning.Id, 30),
            P("Bulaşık Süngeri 3'lü", 19.90m, 0.20m, cleaning.Id, 25),

            // Kahvaltılık
            P("Bal 450g", 129.00m, 0.01m, breakfast.Id, 15),
            P("Vişne Reçeli 380g", 49.90m, 0.10m, breakfast.Id, 20),
            P("Siyah Zeytin 500g", 89.90m, 0.01m, breakfast.Id, 16),
            P("Tahin 300g", 64.90m, 0.01m, breakfast.Id, 12),
            P("Mısır Gevreği 300g", 54.90m, 0.10m, breakfast.Id, 4),

            // Bakliyat & Makarna
            P("Pirinç 1kg", 64.90m, 0.01m, pantry.Id, 40),
            P("Makarna 500g", 17.90m, 0.01m, pantry.Id, 80),
            P("Kırmızı Mercimek 1kg", 49.90m, 0.01m, pantry.Id, 30),
            P("Nohut 1kg", 54.90m, 0.01m, pantry.Id, 25),
            P("Un 1kg", 24.90m, 0.01m, pantry.Id, 35),
            P("Toz Şeker 1kg", 39.90m, 0.01m, pantry.Id, 50),
            P("Ayçiçek Yağı 1L", 79.90m, 0.01m, pantry.Id, 28),
            P("Domates Salçası 700g", 44.90m, 0.01m, pantry.Id, 22),

            // Dondurulmuş
            P("Dondurma 1L", 69.90m, 0.10m, frozen.Id, 18),
            P("Donuk Pizza", 89.90m, 0.10m, frozen.Id, 5),
            P("Donuk Patates 1kg", 54.90m, 0.10m, frozen.Id, 24),
            P("Donuk Karışık Sebze 1kg", 44.90m, 0.01m, frozen.Id, 16),

            // Kişisel Bakım
            P("Şampuan 400ml", 79.90m, 0.20m, care.Id, 20),
            P("Diş Macunu 100ml", 44.90m, 0.20m, care.Id, 30),
            P("Sabun 4'lü", 39.90m, 0.20m, care.Id, 26),
            P("Tuvalet Kağıdı 16'lı", 119.00m, 0.20m, care.Id, 14),
            P("Kağıt Havlu 6'lı", 89.90m, 0.20m, care.Id, 3),

            // Manav (tartılı)
            W("00001", "Domates", 21.90m, 0.01m, produce.Id, 35),
            W("00002", "Elma", 18.50m, 0.01m, produce.Id, 50),
            W("00003", "Muz", 39.90m, 0.01m, produce.Id, 22),
            W("00004", "Salatalık", 16.90m, 0.01m, produce.Id, 28),
            W("00005", "Patates", 14.90m, 0.01m, produce.Id, 80),
            W("00006", "Soğan", 12.90m, 0.01m, produce.Id, 70),
            W("00007", "Portakal", 22.90m, 0.01m, produce.Id, 40),
            W("00008", "Yeşil Biber", 34.90m, 0.01m, produce.Id, 12),

            // Kasap (tartılı)
            W("00010", "Dana Kıyma", 289.00m, 0.01m, butcher.Id, 14),
            W("00011", "Tavuk Göğsü", 149.00m, 0.01m, butcher.Id, 20),
            W("00012", "Kuzu Pirzola", 459.00m, 0.01m, butcher.Id, 4),
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Appends the EAN-13 check digit to a 12-digit base.</summary>
    private static string Ean13(string base12)
    {
        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            var d = base12[i] - '0';
            sum += (i % 2 == 0) ? d : d * 3;
        }
        var check = (10 - (sum % 10)) % 10;
        return base12 + check;
    }
}
