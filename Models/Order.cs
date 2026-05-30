namespace WebShop.Models;

public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";

    public List<OrderItem> Items { get; set; } = [];
}
