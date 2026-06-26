namespace Restaurant.Api.Models;

public class RawMaterial
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public List<ProductIngredient> ProductIngredients { get; set; } = [];
}
