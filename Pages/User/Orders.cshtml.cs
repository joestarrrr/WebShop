using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebShop.Services;

namespace WebShop.Pages.User;

[Authorize]
public class OrdersModel : PageModel
{
    private readonly IShopService _shopService;

    public OrdersModel(IShopService shopService)
    {
        _shopService = shopService;
    }

    public IReadOnlyList<OrderDto> Orders { get; private set; } = [];
    public IReadOnlyList<CartItemDto> CartItems { get; private set; } = [];
    public string StatusMessage { get; private set; } = string.Empty;

    public void OnGet()
    {
        LoadData();
    }

    public IActionResult OnPostCreateOrder()
    {
        var userId = GetUserId();
        var order = _shopService.CreateOrderFromCart(userId);

        StatusMessage = order is null
            ? "Cart is empty. Add products first."
            : $"Order #{order.Id} created successfully.";

        LoadData();
        return Page();
    }

    public IActionResult OnPostIncreaseCartItem(int cartItemId)
    {
        var userId = GetUserId();
        var ok = _shopService.IncreaseCartItemQuantity(userId, cartItemId);
        StatusMessage = ok ? "Quantity increased." : "Cart item not found.";
        LoadData();
        return Page();
    }

    public IActionResult OnPostDecreaseCartItem(int cartItemId)
    {
        var userId = GetUserId();
        var ok = _shopService.DecreaseCartItemQuantity(userId, cartItemId);
        StatusMessage = ok ? "Quantity decreased." : "Cart item not found.";
        LoadData();
        return Page();
    }

    public IActionResult OnPostRemoveCartItem(int cartItemId)
    {
        var userId = GetUserId();
        var ok = _shopService.RemoveCartItem(userId, cartItemId);
        StatusMessage = ok ? "Item removed from cart." : "Cart item not found.";
        LoadData();
        return Page();
    }

    private void LoadData()
    {
        var userId = GetUserId();
        Orders = _shopService.GetOrdersForUser(userId);
        CartItems = _shopService.GetCart(userId);
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "anonymous";
    }
}
