using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.Dtos;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services;

public class OrderService(RestaurantDbContext db) : IOrderService
{
    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync()
    {
        var orders = await db.Orders
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAt)
            .Take(50)
            .ToListAsync();

        return orders.Select(ToDto).ToList();
    }

    public async Task<OrderDto?> GetOrderAsync(int id)
    {
        var order = await GetOrderEntityAsync(id);
        return order is null ? null : ToDto(order);
    }

    public async Task<Order?> GetOrderEntityAsync(int id)
    {
        return await db.Orders
            .Include(order => order.Items.OrderBy(item => item.Id))
            .FirstOrDefaultAsync(order => order.Id == id);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("El pedido necesita al menos un producto.");
        }

        var productIds = request.Items.Select(item => item.ProductId).Distinct().ToList();
        var products = await db.Products
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id);

        foreach (var item in request.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product) || !product.IsActive)
            {
                throw new InvalidOperationException("Hay productos inexistentes o inactivos en el pedido.");
            }

            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException("Las cantidades deben ser mayores a cero.");
            }

            if (product.Stock < item.Quantity)
            {
                throw new InvalidOperationException($"Stock insuficiente para {product.Name}.");
            }
        }

        var order = new Order
        {
            TableName = request.TableName.Trim(),
            Notes = request.Notes.Trim(),
            Status = OrderStatus.Open,
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (var item in request.Items)
        {
            var product = products[item.ProductId];
            product.Stock -= item.Quantity;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Measure = product.Measure,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        order.Total = order.Items.Sum(item => item.UnitPrice * item.Quantity);
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        db.ActivityLogs.Add(new ActivityLog
        {
            Type = "invoice-created",
            Description = $"Se creo la factura del pedido #{order.Id} por ${order.Total:0.##}.",
            CreatedAt = order.CreatedAt
        });
        await db.SaveChangesAsync();

        return ToDto(order);
    }

    public async Task<OrderDto?> UpdateStatusAsync(int id, OrderStatus status)
    {
        var order = await db.Orders.Include(item => item.Items).FirstOrDefaultAsync(item => item.Id == id);
        if (order is null)
        {
            return null;
        }

        order.Status = status;
        await db.SaveChangesAsync();
        return ToDto(order);
    }

    private static OrderDto ToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.TableName,
            order.Notes,
            order.Total,
            order.Status,
            order.CreatedAt,
            order.Items.Select(item => new OrderItemDto(
                item.Id,
                item.ProductId,
                item.ProductName,
                item.Measure,
                item.Quantity,
                item.UnitPrice,
                item.Quantity * item.UnitPrice)).ToList(),
            $"/api/orders/{order.Id}/invoice.pdf");
    }
}
