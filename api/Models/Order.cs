namespace Restaurant.Api.Models;

public class Order
{
    public int Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public int? CashRegisterId { get; set; }
    public CashRegister? CashRegister { get; set; }
    public List<OrderItem> Items { get; set; } = [];
}

public enum OrderStatus
{
    Open,
    Paid,
    Cancelled
}
