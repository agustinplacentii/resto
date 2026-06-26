using Restaurant.Api.Dtos;

namespace Restaurant.Api.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductGroupDto>> GetGroupsAsync();
    Task<ProductGroupDto> CreateGroupAsync(ProductGroupRequest request);
    Task<ProductGroupDto?> UpdateGroupAsync(int id, ProductGroupRequest request);
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(int? groupId);
    Task<ProductDto?> GetProductAsync(int id);
    Task<ProductDto> CreateProductAsync(ProductRequest request);
    Task<ProductDto?> UpdateProductAsync(int id, ProductRequest request);
    Task<ProductDto?> AddStockAsync(int id, StockAdjustmentRequest request);
    Task<ProductDto?> DiscountStockAsync(int id, StockAdjustmentRequest request);
    Task<bool?> DeleteProductAsync(int id);
}
