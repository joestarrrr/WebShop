using Microsoft.AspNetCore.Authorization;
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

    public void OnGet()
    {
        Orders = _shopService.GetAllOrders();
    }
}
