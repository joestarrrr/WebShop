namespace WebShop.Services;

public interface IShopService
{
    IReadOnlyList<ProductDto> GetProducts();
    ProductDto? GetProductById(int id);
    ProductDto AddProduct(CreateProductRequest request);
    bool UpdateProduct(int id, UpdateProductRequest request);
    bool DeleteProduct(int id);

    bool AddToCart(string userId, int productId);
    bool IncreaseCartItemQuantity(string userId, int cartItemId);
    bool DecreaseCartItemQuantity(string userId, int cartItemId);
    bool RemoveCartItem(string userId, int cartItemId);
    IReadOnlyList<CartItemDto> GetCart(string userId);

    OrderDto? CreateOrderFromCart(string userId);
    IReadOnlyList<OrderDto> GetOrdersForUser(string userId);
    IReadOnlyList<OrderDto> GetAllOrders();
    bool UpdateOrderStatus(int orderId, string status);
}
