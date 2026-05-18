using Restaurant.Api.Models;

namespace Restaurant.Api.Dtos;

public record CreateOrderRequest(string TableName, string Notes, List<CreateOrderItemRequest> Items);

public record CreateOrderItemRequest(int ProductId, int Quantity);

public record UpdateOrderStatusRequest(OrderStatus Status);

public record OrderDto(
    int Id,
    string TableName,
    string Notes,
    decimal Total,
    OrderStatus Status,
    DateTimeOffset CreatedAt,
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
