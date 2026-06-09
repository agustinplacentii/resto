using Restaurant.Api.Models;

namespace Restaurant.Api.Dtos;

public record CreateOrderRequest(string? TableName, string? CustomerName, string? Notes, bool PayNow, List<CreateOrderItemRequest> Items);

public record CreateOrderItemRequest(int ProductId, int Quantity);

public record UpdateOrderStatusRequest(OrderStatus Status);

public record AccountSearchDto(
    string SearchType,
    string SearchValue,
    decimal Total,
    IReadOnlyList<OrderDto> Orders);

public record OrderDto(
    int Id,
    string TableName,
    string CustomerName,
    string Notes,
    decimal Total,
    OrderStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaidAt,
    IReadOnlyList<OrderItemDto> Items,
    string InvoiceUrl);

public record OrderItemDto(
    int Id,
    int ProductId,
    string ProductName,
    string Measure,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal);
