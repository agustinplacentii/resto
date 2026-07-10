namespace Restaurant.Api.Dtos;

public record OpenCashRegisterRequest(DateTime OpenedAt);

public record CashRegisterDto(
    int Id,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal Total,
    IReadOnlyList<CashRegisterItemSummaryDto> ItemSummaries,
    IReadOnlyList<OrderDto> PaidOrders);

public record CashRegisterItemSummaryDto(
    int ProductId,
    string ProductName,
    string Measure,
    int Quantity,
    decimal Total);
