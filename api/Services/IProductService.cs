using Restaurant.Api.Dtos;

namespace Restaurant.Api.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductGroupDto>> GetGroupsAsync();
    Task<ProductGroupDto> CreateGroupAsync(ProductGroupRequest request);
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(int? groupId);
    Task<ProductDto?> GetProductAsync(int id);
    Task<ProductDto> CreateProductAsync(ProductRequest request);
    Task<ProductDto?> UpdateProductAsync(int id, ProductRequest request);
}
