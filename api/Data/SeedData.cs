using Restaurant.Api.Models;

namespace Restaurant.Api.Data;

public static class SeedData
{
    public static void EnsureSeeded(RestaurantDbContext db)
    {
        var groups = EnsureGroups(db);

        EnsureProduct(db, groups["Pizzas"], "Pizza muzzarella", "Pizzas", "8 porciones", 8200, 12);
        EnsureProduct(db, groups["Pizzas"], "Pizza napolitana", "Pizzas", "8 porciones", 9400, 10);
        EnsureProduct(db, groups["Pizzas"], "Pizza 4 quesos", "Pizzas", "8 porciones", 10800, 8);
        EnsureProduct(db, groups["Pizzas"], "Pizza fugazzeta", "Pizzas", "8 porciones", 9900, 9);

        EnsureProduct(db, groups["Cervezas"], "Cerveza rubia", "Cervezas", "Pinta 500ml", 2600, 40);
        EnsureProduct(db, groups["Cervezas"], "Cerveza roja", "Cervezas", "Pinta 500ml", 2900, 28);
        EnsureProduct(db, groups["Cervezas"], "Cerveza IPA", "Cervezas", "Pinta 500ml", 3200, 25);
        EnsureProduct(db, groups["Cervezas"], "Cerveza sin alcohol", "Cervezas", "Botella 355ml", 2400, 20);

        EnsureProduct(db, groups["Tragos"], "Fernet con cola", "Tragos", "Vaso", 3600, 32);
        EnsureProduct(db, groups["Tragos"], "Gin tonic", "Tragos", "Copa", 4200, 24);
        EnsureProduct(db, groups["Tragos"], "Aperol spritz", "Tragos", "Copa", 4500, 20);
        EnsureProduct(db, groups["Tragos"], "Campari naranja", "Tragos", "Vaso", 3900, 22);

        EnsureProduct(db, groups["Platos"], "Milanesa con fritas", "Platos", "Plato", 7800, 20);
        EnsureProduct(db, groups["Platos"], "Hamburguesa completa", "Platos", "Unidad", 6900, 18);
        EnsureProduct(db, groups["Entradas"], "Empanada carne", "Entradas", "Unidad", 1200, 60);
        EnsureProduct(db, groups["Bebidas"], "Gaseosa 500ml", "Bebidas", "Botella 500ml", 1800, 45);
        EnsureProduct(db, groups["Bebidas"], "Agua mineral", "Bebidas", "Botella 500ml", 1400, 50);

        db.SaveChanges();
    }

    private static Dictionary<string, ProductGroup> EnsureGroups(RestaurantDbContext db)
    {
        var definitions = new[]
        {
            new ProductGroup { Name = "Pizzas", Description = "Muzzarella, napolitana, 4 quesos y mas variedades." },
            new ProductGroup { Name = "Cervezas", Description = "Pintas y botellas para cargar rapido al pedido." },
            new ProductGroup { Name = "Tragos", Description = "Cocteles y tragos por vaso o copa." },
            new ProductGroup { Name = "Platos", Description = "Principales de cocina." },
            new ProductGroup { Name = "Entradas", Description = "Para agregar unidades al pedido." },
            new ProductGroup { Name = "Bebidas", Description = "Sin alcohol y botellas chicas." }
        };

        foreach (var group in definitions)
        {
            if (!db.ProductGroups.Any(existing => existing.Name == group.Name))
            {
                db.ProductGroups.Add(group);
            }
        }

        db.SaveChanges();
        return db.ProductGroups.ToDictionary(group => group.Name);
    }

    private static void EnsureProduct(
        RestaurantDbContext db,
        ProductGroup group,
        string name,
        string category,
        string measure,
        decimal price,
        int stock)
    {
        var product = db.Products.FirstOrDefault(existing => existing.Name == name);
        if (product is null)
        {
            db.Products.Add(new Product
            {
                Name = name,
                Category = category,
                Measure = measure,
                Price = price,
                Stock = stock,
                IsActive = true,
                ProductGroupId = group.Id
            });
            return;
        }

        product.Category = category;
        product.Measure = string.IsNullOrWhiteSpace(product.Measure) ? measure : product.Measure;
        product.ProductGroupId ??= group.Id;
    }
}
