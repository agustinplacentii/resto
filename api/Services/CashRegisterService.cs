using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.Dtos;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services;

public class CashRegisterService(RestaurantDbContext db) : ICashRegisterService
{
    public async Task<CashRegisterDto?> GetCurrentAsync()
    {
        var cashRegister = await db.CashRegisters
            .Where(item => item.ClosedAt == null)
            .OrderByDescending(item => item.OpenedAt)
            .FirstOrDefaultAsync();

        return cashRegister is null ? null : await ToDtoAsync(cashRegister);
    }

    public async Task<IReadOnlyList<CashRegisterDto>> GetClosedAsync()
    {
        var cashRegisters = await db.CashRegisters
            .Where(item => item.ClosedAt != null)
            .OrderByDescending(item => item.ClosedAt)
            .Take(30)
            .ToListAsync();

        var result = new List<CashRegisterDto>();
        foreach (var cashRegister in cashRegisters)
        {
            result.Add(await ToDtoAsync(cashRegister));
        }

        return result;
    }

    public async Task<CashRegisterDto> OpenAsync(OpenCashRegisterRequest request)
    {
        var hasOpenRegister = await db.CashRegisters.AnyAsync(item => item.ClosedAt == null);
        if (hasOpenRegister)
        {
            throw new InvalidOperationException("Ya hay una caja abierta.");
        }

        var openedAt = request.OpenedAt == default ? DateTimeOffset.UtcNow : request.OpenedAt.ToUniversalTime();
        if (openedAt > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            throw new InvalidOperationException("La apertura de caja no puede ser futura.");
        }

        var cashRegister = new CashRegister
        {
            OpenedAt = openedAt,
            Total = 0
        };

        db.CashRegisters.Add(cashRegister);
        db.ActivityLogs.Add(new ActivityLog
        {
            Type = "cash-opened",
            Description = $"Se abrio la caja desde {openedAt.LocalDateTime:g}.",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        return await ToDtoAsync(cashRegister);
    }

    public async Task<CashRegisterDto?> CloseAsync()
    {
        var cashRegister = await db.CashRegisters
            .Where(item => item.ClosedAt == null)
            .OrderByDescending(item => item.OpenedAt)
            .FirstOrDefaultAsync();

        if (cashRegister is null)
        {
            return null;
        }

        var closedAt = DateTimeOffset.UtcNow;
        var paidOrders = await GetPaidOrdersAsync(cashRegister.OpenedAt, closedAt);
        cashRegister.ClosedAt = closedAt;
        cashRegister.Total = paidOrders.Sum(order => order.Total);
        foreach (var order in paidOrders)
        {
            order.CashRegisterId = cashRegister.Id;
        }

        db.ActivityLogs.Add(new ActivityLog
        {
            Type = "cash-closed",
            Description = $"Se cerro la caja con {paidOrders.Count} pedidos cobrados por ${cashRegister.Total:0.##}.",
            CreatedAt = closedAt
        });
        await db.SaveChangesAsync();

        return ToDto(cashRegister, paidOrders);
    }

    private async Task<CashRegisterDto> ToDtoAsync(CashRegister cashRegister)
    {
        var paidOrders = cashRegister.ClosedAt is null
            ? await GetPaidOrdersAsync(cashRegister.OpenedAt, DateTimeOffset.UtcNow)
            : await GetClosedPaidOrdersAsync(cashRegister.Id);

        return ToDto(cashRegister, paidOrders);
    }

    private async Task<List<Order>> GetPaidOrdersAsync(DateTimeOffset from, DateTimeOffset until)
    {
        return await db.Orders
            .Include(order => order.Items.OrderBy(item => item.Id))
            .Where(order => order.Status == OrderStatus.Paid && order.CashRegisterId == null && order.CreatedAt >= from && order.CreatedAt <= until)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync();
    }

    private async Task<List<Order>> GetClosedPaidOrdersAsync(int cashRegisterId)
    {
        return await db.Orders
            .Include(order => order.Items.OrderBy(item => item.Id))
            .Where(order => order.CashRegisterId == cashRegisterId)
            .OrderByDescending(order => order.PaidAt)
            .ToListAsync();
    }

    private static CashRegisterDto ToDto(CashRegister cashRegister, IReadOnlyList<Order> paidOrders)
    {
        var total = cashRegister.ClosedAt is null
            ? paidOrders.Sum(order => order.Total)
            : cashRegister.Total;

        return new CashRegisterDto(
            cashRegister.Id,
            cashRegister.OpenedAt,
            cashRegister.ClosedAt,
            total,
            BuildItemSummaries(paidOrders),
            paidOrders.Select(ToOrderDto).ToList());
    }

    private static List<CashRegisterItemSummaryDto> BuildItemSummaries(IReadOnlyList<Order> paidOrders)
    {
        return paidOrders
            .SelectMany(order => order.Items)
            .GroupBy(item => new { item.ProductId, item.ProductName, item.Measure })
            .Select(group => new CashRegisterItemSummaryDto(
                group.Key.ProductId,
                group.Key.ProductName,
                group.Key.Measure,
                group.Sum(item => item.Quantity),
                group.Sum(item => item.Quantity * item.UnitPrice)))
            .OrderBy(item => item.ProductName)
            .ToList();
    }

    private static OrderDto ToOrderDto(Order order)
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
