namespace Restaurant.Api.Models;

public class ProductIngredient
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int RawMaterialId { get; set; }
    public RawMaterial? RawMaterial { get; set; }
    public decimal Quantity { get; set; }
}
