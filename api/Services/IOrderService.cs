using Restaurant.Api.Dtos;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetOrdersAsync();
    Task<OrderDto?> GetOrderAsync(int id);
    Task<Order?> GetOrderEntityAsync(int id);
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderDto?> UpdateStatusAsync(int id, OrderStatus status);
}
