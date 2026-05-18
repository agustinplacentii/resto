using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.Dtos;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services;

public class ProductService(RestaurantDbContext db) : IProductService
{
    public async Task<IReadOnlyList<ProductGroupDto>> GetGroupsAsync()
    {
        return await db.ProductGroups
            .OrderBy(group => group.Name)
            .Select(group => new ProductGroupDto(
                group.Id,
                group.Name,
                group.Description,
                group.Products.Count(product => product.IsActive)))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(int? groupId)
    {
        var query = db.Products.Include(product => product.ProductGroup).AsQueryable();

        if (groupId.HasValue)
        {
            query = query.Where(product => product.ProductGroupId == groupId.Value);
        }

        var products = await query
            .OrderBy(product => product.Name)
            .ToListAsync();

        return products.Select(ToDto).ToList();
    }

    public async Task<ProductDto?> GetProductAsync(int id)
    {
        var product = await db.Products
            .Include(item => item.ProductGroup)
            .FirstOrDefaultAsync(item => item.Id == id);

        return product is null ? null : ToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(ProductRequest request)
    {
        var product = new Product();
        ApplyRequest(product, request);

        db.Products.Add(product);
        await db.SaveChangesAsync();
        await db.Entry(product).Reference(item => item.ProductGroup).LoadAsync();

        return ToDto(product);
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, ProductRequest request)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
        {
            return null;
        }

        ApplyRequest(product, request);
        await db.SaveChangesAsync();
        await db.Entry(product).Reference(item => item.ProductGroup).LoadAsync();

        return ToDto(product);
    }

    private static void ApplyRequest(Product product, ProductRequest request)
    {
        product.Name = request.Name.Trim();
        product.Category = request.Category.Trim();
        product.Measure = request.Measure.Trim();
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.IsActive = request.IsActive;
        product.ProductGroupId = request.ProductGroupId;
    }

    public static ProductDto ToDto(Product product)
    {
        return new ProductDto(
            product.Id,
            product.Name,
            product.Category,
            product.Measure,
            product.Price,
            product.Stock,
            product.IsActive,
            product.ProductGroupId,
            product.ProductGroup?.Name);
    }
}
