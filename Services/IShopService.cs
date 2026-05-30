namespace WebShop.Services;

public interface IShopService
{
    IReadOnlyList<ProductDto> GetProducts();
    ProductDto? GetProductById(int id);
    ProductDto AddProduct(CreateProductRequest request);
    bool UpdateProduct(int id, UpdateProductRequest request);
    bool DeleteProduct(int id);

    bool AddToCart(string userId, int productId);
    IReadOnlyList<CartItemDto> GetCart(string userId);

    OrderDto? CreateOrderFromCart(string userId);
    IReadOnlyList<OrderDto> GetOrdersForUser(string userId);
    IReadOnlyList<OrderDto> GetAllOrders();
}
