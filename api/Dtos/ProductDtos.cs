namespace Restaurant.Api.Dtos;

public record ProductGroupDto(int Id, string Name, string Description, int ProductCount);

public record ProductGroupRequest(string Name, string Description);

public record ProductIngredientDto(int RawMaterialId, string RawMaterialName, string Unit, decimal Quantity);

public record ProductIngredientRequest(int RawMaterialId, decimal Quantity);

public record ProductDto(
    int Id,
    string Name,
    string Category,
    string Measure,
    decimal Price,
    int Stock,
    bool IsActive,
    int? ProductGroupId,
    string? ProductGroupName
);

public record ProductRequest(
    string Name,
    string Category,
    string Measure,
    decimal Price,
    int Stock,
    bool IsActive = true,
    int? ProductGroupId = null
);

public record StockAdjustmentRequest(int Quantity, string Reason);

public record RawMaterialDto(int Id, string Name, string Unit, decimal Quantity);

public record RawMaterialRequest(string Name, string Unit, decimal Quantity);