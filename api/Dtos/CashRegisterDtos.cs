namespace Restaurant.Api.Dtos;

public record OpenCashRegisterRequest(DateTimeOffset OpenedAt);

public record CashRegisterDto(
    int Id,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt,
    decimal Total,
    IReadOnlyList<CashRegisterItemSummaryDto> ItemSummaries,
    IReadOnlyList<OrderDto> PaidOrders);

public record CashRegisterItemSummaryDto(
    int ProductId,
    string ProductName,
    string Measure,
    int Quantity,
    decimal Total);
