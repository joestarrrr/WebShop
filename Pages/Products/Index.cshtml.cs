using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebShop.Services;

namespace WebShop.Pages.Products;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IShopService _shopService;

    public IndexModel(IShopService shopService)
    {
        _shopService = shopService;
    }

    public IReadOnlyList<ProductDto> Products { get; private set; } = [];
    public string StatusMessage { get; private set; } = string.Empty;

    public void OnGet()
    {
        Products = _shopService.GetProducts();
    }

    public IActionResult OnPostAddToCart(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "anonymous";
        var added = _shopService.AddToCart(userId, productId);

        StatusMessage = added
            ? "Product added to cart."
            : "Product not found.";

        Products = _shopService.GetProducts();
        return Page();
    }
}
