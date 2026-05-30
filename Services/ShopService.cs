using Microsoft.EntityFrameworkCore;
using WebShop.Data;
using WebShop.Models;

namespace WebShop.Services;

public record ProductDto(int Id, string Name, decimal Price, string Description);
public record CreateProductRequest(string Name, decimal Price, string Description);
public record UpdateProductRequest(string Name, decimal Price, string Description);
public record CartItemDto(int ProductId, string Name, decimal Price);
public record OrderItemDto(int ProductId, string ProductName, decimal UnitPrice);
public record OrderDto(int Id, string UserId, DateTime CreatedAtUtc, decimal TotalAmount, IReadOnlyList<OrderItemDto> Items);

public class ShopService : IShopService
{
    private readonly ApplicationDbContext _dbContext;

    public ShopService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyList<ProductDto> GetProducts()
    {
        return _dbContext.Products
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Description))
            .OrderBy(p => p.Id)
            .ToList();
    }

    public ProductDto? GetProductById(int id)
    {
        return _dbContext.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Description))
            .FirstOrDefault();
    }

    public ProductDto AddProduct(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description
        };

        _dbContext.Products.Add(product);
        _dbContext.SaveChanges();

        return new ProductDto(product.Id, product.Name, product.Price, product.Description);
    }

    public bool UpdateProduct(int id, UpdateProductRequest request)
    {
        var product = _dbContext.Products.FirstOrDefault(p => p.Id == id);
        if (product is null)
        {
            return false;
        }

        product.Name = request.Name;
        product.Price = request.Price;
        product.Description = request.Description;
        _dbContext.SaveChanges();

        return true;
    }

    public bool DeleteProduct(int id)
    {
        var product = _dbContext.Products.FirstOrDefault(p => p.Id == id);
        if (product is null)
        {
            return false;
        }

        _dbContext.Products.Remove(product);
        _dbContext.SaveChanges();

        return true;
    }

    public bool AddToCart(string userId, int productId)
    {
        var product = _dbContext.Products.FirstOrDefault(p => p.Id == productId);

        if (product is null)
        {
            return false;
        }

        var cartItem = new CartItem
        {
            UserId = userId,
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = 1
        };

        _dbContext.CartItems.Add(cartItem);
        _dbContext.SaveChanges();

        return true;
    }

    public IReadOnlyList<CartItemDto> GetCart(string userId)
    {
        return _dbContext.CartItems
            .AsNoTracking()
            .Where(ci => ci.UserId == userId)
            .Select(ci => new CartItemDto(ci.ProductId, ci.ProductName, ci.UnitPrice * ci.Quantity))
            .ToList();
    }

    public OrderDto? CreateOrderFromCart(string userId)
    {
        var cartRows = _dbContext.CartItems
            .Where(ci => ci.UserId == userId)
            .ToList();

        if (cartRows.Count == 0)
        {
            return null;
        }

        var items = cartRows
            .Select(ci => new CartItemDto(ci.ProductId, ci.ProductName, ci.UnitPrice * ci.Quantity))
            .ToList();

        var order = new Order
        {
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            TotalAmount = items.Sum(i => i.Price),
            Items = items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.Name,
                UnitPrice = i.Price
            }).ToList()
        };

        _dbContext.Orders.Add(order);
        _dbContext.CartItems.RemoveRange(cartRows);
        _dbContext.SaveChanges();

        return new OrderDto(
            order.Id,
            order.UserId,
            order.CreatedAtUtc,
            order.TotalAmount,
            order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice)).ToList()
        );
    }

    public IReadOnlyList<OrderDto> GetOrdersForUser(string userId)
    {
        return _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new OrderDto(
                o.Id,
                o.UserId,
                o.CreatedAtUtc,
                o.TotalAmount,
                o.Items
                    .Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice))
                    .ToList()
            ))
            .ToList();
    }

    public IReadOnlyList<OrderDto> GetAllOrders()
    {
        return _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new OrderDto(
                o.Id,
                o.UserId,
                o.CreatedAtUtc,
                o.TotalAmount,
                o.Items
                    .Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice))
                    .ToList()
            ))
            .ToList();
    }
}
