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

    public async Task<AccountSearchDto> SearchOpenAccountAsync(string searchType, string searchValue)
    {
        var normalizedType = searchType.Trim().ToLowerInvariant();
        var value = searchValue.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Escribi una mesa o cliente para buscar la cuenta.");
        }

        if (normalizedType is not "table" and not "customer")
        {
            throw new InvalidOperationException("Selecciona si queres buscar por mesa o por cliente.");
        }

        var lowered = value.ToLower();
        var query = db.Orders
            .Include(order => order.Items.OrderBy(item => item.Id))
            .Where(order => order.Status == OrderStatus.Open);

        query = normalizedType == "table"
            ? query.Where(order => order.TableName.ToLower() == lowered)
            : query.Where(order => order.CustomerName.ToLower().Contains(lowered));

        var orders = await query
            .OrderBy(order => order.CreatedAt)
            .ToListAsync();

        return new AccountSearchDto(normalizedType, value, orders.Sum(order => order.Total), orders.Select(ToDto).ToList());
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
            TableName = (request.TableName ?? string.Empty).Trim(),
            CustomerName = (request.CustomerName ?? string.Empty).Trim(),
            Notes = (request.Notes ?? string.Empty).Trim(),
            Status = request.PayNow ? OrderStatus.Paid : OrderStatus.Open,
            PaidAt = request.PayNow ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow
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

        if (order.CashRegisterId is not null && status != OrderStatus.Paid)
        {
            throw new InvalidOperationException("El pedido ya fue incluido en una caja cerrada.");
        }

        if (order.Status == OrderStatus.Paid && status == OrderStatus.Open)
        {
            throw new InvalidOperationException("Un pedido pagado solo se puede mantener pagado o cancelar.");
        }

        if (order.Status == OrderStatus.Cancelled && status == OrderStatus.Open)
        {
            throw new InvalidOperationException("Un pedido cancelado solo se puede volver a marcar como pagado.");
        }

        order.Status = status;
        order.PaidAt = status == OrderStatus.Paid
            ? order.PaidAt ?? DateTime.UtcNow
            : null;

        await db.SaveChangesAsync();
        return ToDto(order);
    }

    private static OrderDto ToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.TableName,
            order.CustomerName,
            order.Notes,
            order.Total,
            order.Status,
            order.CreatedAt,
            order.PaidAt,
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
