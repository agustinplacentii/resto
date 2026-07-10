namespace Restaurant.Api.Models;

public class CashRegister
{
    public int Id { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal Total { get; set; }
    public List<Order> Orders { get; set; } = [];
}
