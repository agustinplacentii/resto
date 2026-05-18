namespace Restaurant.Api.Models;

public class Order
{
    public int Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Open;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<OrderItem> Items { get; set; } = [];
}

public enum OrderStatus
{
    Open,
    Paid,
    Cancelled
}

