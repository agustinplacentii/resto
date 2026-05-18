using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.Dtos;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services;

public class ProductService(RestaurantDbContext db) : IProductService
{
    public async Task<IReadOnlyList<ProductGroupDto>> GetGroupsAsync()
    {
        var groups = await db.ProductGroups
            .OrderBy(group => group.Name)
            .ToListAsync();

        return groups
            .Select(group => new ProductGroupDto(
                group.Id,
                group.Name,
                group.Description,
                db.Products.Count(product => product.ProductGroupId == group.Id && product.IsActive)))
            .ToList();
    }

    public async Task<ProductGroupDto> CreateGroupAsync(ProductGroupRequest request)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre de la categoria es obligatorio.");
        }

        var existing = await db.ProductGroups.FirstOrDefaultAsync(group => group.Name == name);
        if (existing is not null)
        {
            return new ProductGroupDto(
                existing.Id,
                existing.Name,
                existing.Description,
                await db.Products.CountAsync(product => product.ProductGroupId == existing.Id && product.IsActive));
        }

        var group = new ProductGroup
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? $"Productos de {name}."
                : request.Description.Trim()
        };

        db.ProductGroups.Add(group);
        await db.SaveChangesAsync();

        return new ProductGroupDto(group.Id, group.Name, group.Description, 0);
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
        if (request.ProductGroupId is null or 0)
        {
            throw new InvalidOperationException("Selecciona una categoria para guardar el producto.");
        }

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

    public async Task<ProductDto?> DiscountStockAsync(int id, StockAdjustmentRequest request)
    {
        var product = await db.Products
            .Include(item => item.ProductGroup)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (product is null)
        {
            return null;
        }

        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException("La cantidad a descontar debe ser mayor a cero.");
        }

        var reason = request.Reason.Trim();
        if (reason is not "courtesy" and not "damaged")
        {
            throw new InvalidOperationException("Selecciona un motivo valido para descontar stock.");
        }

        if (product.Stock < request.Quantity)
        {
            throw new InvalidOperationException($"Stock insuficiente para {product.Name}.");
        }

        product.Stock -= request.Quantity;
        await db.SaveChangesAsync();

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
        product.ProductGroupId = request.ProductGroupId is 0 ? null : request.ProductGroupId;
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
