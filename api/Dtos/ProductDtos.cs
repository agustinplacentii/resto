namespace Restaurant.Api.Dtos;

public record ProductGroupDto(int Id, string Name, string Description, int ProductCount);

public record ProductGroupRequest(string Name, string Description);

public record ProductDto(
    int Id,
    string Name,
    string Category,
    string Measure,
    decimal Price,
    int Stock,
    bool IsActive,
    int? ProductGroupId,
    string? ProductGroupName);

public record ProductRequest(
    string Name,
    string Category,
    string Measure,
    decimal Price,
    int Stock,
    bool IsActive = true,
    int? ProductGroupId = null);

public record StockAdjustmentRequest(int Quantity, string Reason);
