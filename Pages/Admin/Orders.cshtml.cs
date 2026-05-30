using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebShop.Services;

namespace WebShop.Pages.Admin;

[Authorize(Roles = "Admin")]
public class OrdersModel : PageModel
{
    private readonly IShopService _shopService;

    public OrdersModel(IShopService shopService)
    {
        _shopService = shopService;
    }

    public IReadOnlyList<OrderDto> Orders { get; private set; } = [];
    public string StatusMessage { get; private set; } = string.Empty;

    public void OnGet()
    {
        Orders = _shopService.GetAllOrders();
    }

    public IActionResult OnPostUpdateStatus(int orderId, string status)
    {
        var updated = _shopService.UpdateOrderStatus(orderId, status);
        StatusMessage = updated ? "Order status updated." : "Could not update order status.";
        Orders = _shopService.GetAllOrders();
        return Page();
    }
}
