namespace Restaurant.Api.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Measure { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool RequiresStock { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int? ProductGroupId { get; set; }
    public ProductGroup? ProductGroup { get; set; }
    public List<ProductIngredient> Ingredients { get; set; } = [];
}
