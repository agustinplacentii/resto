namespace Restaurant.Api.Models;

public class CashRegister
{
    public int Id { get; set; }
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public decimal Total { get; set; }
    public List<Order> Orders { get; set; } = [];
}
