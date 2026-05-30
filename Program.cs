// =============================================================================
// Program.cs — Application entry point
// =============================================================================
//
// This is where the entire application is configured and started.
// It has two phases:
//
//   1. BUILDER phase  – register services (dependency injection container)
//   2. APP phase      – configure the HTTP request pipeline (middleware)
//
// "Services" are things like the database, Identity, email sender, etc.
// They are registered once here and then injected wherever they are needed.
//
// "Middleware" is a chain of components that process every HTTP request in
// order (authentication → routing → endpoint execution → response).
// =============================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebShop.Data;
using WebShop.Services;

// ---------------------------------------------------------------------------
// BUILDER PHASE — register all services
// ---------------------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

// 1. Connect Entity Framework Core to the SQL Server LocalDB database.
//    The connection string "DefaultConnection" is defined in appsettings.json.
//    EF Core uses ApplicationDbContext to know which tables exist.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Set up ASP.NET Core Identity (user accounts, passwords, login/logout).
//    - AddDefaultIdentity<IdentityUser>() registers the default user type.
//    - AddDefaultTokenProviders() enables email confirmation tokens, etc.
//    - AddEntityFrameworkStores<ApplicationDbContext>() tells Identity to
//      store user data in the same database via ApplicationDbContext.
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Keep password rules simple for development / learning purposes.
    // In a real production app you would want stricter settings.
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// 3. Add Razor Pages support (the page-based UI framework we use).
builder.Services.AddRazorPages();

// 4. Register shop service.
builder.Services.AddScoped<IShopService, ShopService>();

// ---------------------------------------------------------------------------
// APP PHASE — configure the HTTP request pipeline (middleware order matters!)
// ---------------------------------------------------------------------------

var app = builder.Build();

// Seed basic roles and one admin user for development.
await app.SeedRolesAndAdminAsync();

// Show a friendly error page in production; show detailed errors in development.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // HSTS tells browsers to only connect over HTTPS for the next 30 days.
    app.UseHsts();
}

// Redirect http:// requests to https://
app.UseHttpsRedirection();

// Serve static files from wwwroot/ (CSS, JS, images, Bootstrap, etc.)
app.UseStaticFiles();

// Enable routing so the framework can match URLs to Razor Pages.
app.UseRouting();

// Enable authentication (reading the login cookie / JWT token).
// This MUST come before UseAuthorization.
app.UseAuthentication();

// Keep role management simple: every authenticated non-admin user gets User role.
app.Use(async (context, next) =>
{
    var user = context.User;
    if (user.Identity?.IsAuthenticated == true && !user.IsInRole("Admin"))
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var userManager = context.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
            var identityUser = await userManager.FindByIdAsync(userId);

            if (identityUser is not null && !await userManager.IsInRoleAsync(identityUser, "User"))
            {
                await userManager.AddToRoleAsync(identityUser, "User");
            }
        }
    }

    await next();
});

// Enable authorization (checking if the user has permission to access a page).
app.UseAuthorization();

// Map Razor Pages routes.
// Razor Pages routing works by convention:
//   Pages/Index.cshtml          → /
//   Pages/Privacy.cshtml        → /Privacy
//   Pages/Products/List.cshtml  → /Products/List
// No explicit route configuration is needed for simple pages.
app.MapRazorPages();

// ---------------------------------------------------------------------------
// Simple API endpoints
// Guest  -> /api/public/*
// User   -> /api/user/* (logged in)
// Admin  -> /api/admin/* (Admin role)
// ---------------------------------------------------------------------------

var api = app.MapGroup("/api");

api.MapGet("/me", (HttpContext httpContext) =>
{
    var user = httpContext.User;
    var isAuthenticated = user.Identity?.IsAuthenticated ?? false;

    if (!isAuthenticated)
    {
        return Results.Ok(new
        {
            isAuthenticated = false,
            role = "Guest",
            userName = "Guest"
        });
    }

    var isAdmin = user.IsInRole("Admin");
    return Results.Ok(new
    {
        isAuthenticated = true,
        role = isAdmin ? "Admin" : "User",
        userName = user.Identity?.Name ?? "User"
    });
}).AllowAnonymous();

var publicApi = api.MapGroup("/public");
publicApi.MapGet("/products", (IShopService shopService) => Results.Ok(shopService.GetProducts()));
publicApi.MapGet("/products/{id:int}", (int id, IShopService shopService) =>
{
    var product = shopService.GetProductById(id);
    return product is null
        ? Results.NotFound(new { message = "Product not found." })
        : Results.Ok(product);
});

var userApi = api.MapGroup("/user").RequireAuthorization();
userApi.MapGet("/cart", (HttpContext httpContext, IShopService shopService) =>
{
    var userId = GetUserId(httpContext.User);
    return Results.Ok(shopService.GetCart(userId));
});

userApi.MapPost("/cart/{productId:int}", (int productId, HttpContext httpContext, IShopService shopService) =>
{
    var userId = GetUserId(httpContext.User);
    var added = shopService.AddToCart(userId, productId);

    return added
        ? Results.Ok(new { message = "Product added to cart." })
        : Results.NotFound(new { message = "Product not found." });
});

userApi.MapPost("/cart/items/{cartItemId:int}/increase", (int cartItemId, HttpContext httpContext, IShopService shopService) =>
{
    var userId = GetUserId(httpContext.User);
    var ok = shopService.IncreaseCartItemQuantity(userId, cartItemId);

    return ok
        ? Results.Ok(new { message = "Cart item increased." })
        : Results.NotFound(new { message = "Cart item not found." });
});

userApi.MapPost("/cart/items/{cartItemId:int}/decrease", (int cartItemId, HttpContext httpContext, IShopService shopService) =>
{
    var userId = GetUserId(httpContext.User);
    var ok = shopService.DecreaseCartItemQuantity(userId, cartItemId);

    return ok
        ? Results.Ok(new { message = "Cart item decreased." })
        : Results.NotFound(new { message = "Cart item not found." });
});

userApi.MapDelete("/cart/items/{cartItemId:int}", (int cartItemId, HttpContext httpContext, IShopService shopService) =>
{
    var userId = GetUserId(httpContext.User);
    var ok = shopService.RemoveCartItem(userId, cartItemId);

    return ok
        ? Results.Ok(new { message = "Cart item removed." })
        : Results.NotFound(new { message = "Cart item not found." });
});

userApi.MapPost("/orders", (HttpContext httpContext, IShopService shopService) =>
{
    var userId = GetUserId(httpContext.User);
    var order = shopService.CreateOrderFromCart(userId);

    return order is null
        ? Results.BadRequest(new { message = "Cart is empty." })
        : Results.Ok(order);
});

userApi.MapGet("/orders", (HttpContext httpContext, IShopService shopService) =>
{
    var userId = GetUserId(httpContext.User);
    return Results.Ok(shopService.GetOrdersForUser(userId));
});

var adminApi = api.MapGroup("/admin").RequireAuthorization(policy => policy.RequireRole("Admin"));
adminApi.MapPost("/products", (CreateProductRequest request, IShopService shopService) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || request.Price < 0)
    {
        return Results.BadRequest(new { message = "Name is required and price must be >= 0." });
    }

    var product = shopService.AddProduct(request);
    return Results.Created($"/api/public/products/{product.Id}", product);
});

adminApi.MapPut("/products/{id:int}", (int id, UpdateProductRequest request, IShopService shopService) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || request.Price < 0)
    {
        return Results.BadRequest(new { message = "Name is required and price must be >= 0." });
    }

    var updated = shopService.UpdateProduct(id, request);
    return updated
        ? Results.Ok(new { message = "Product updated." })
        : Results.NotFound(new { message = "Product not found." });
});

adminApi.MapDelete("/products/{id:int}", (int id, IShopService shopService) =>
{
    var deleted = shopService.DeleteProduct(id);
    return deleted
        ? Results.Ok(new { message = "Product deleted." })
        : Results.NotFound(new { message = "Product not found." });
});

adminApi.MapGet("/orders", (IShopService shopService) =>
{
    return Results.Ok(shopService.GetAllOrders());
});

adminApi.MapPut("/orders/{id:int}/status", (int id, UpdateOrderStatusRequest request, IShopService shopService) =>
{
    var updated = shopService.UpdateOrderStatus(id, request.Status);
    return updated
        ? Results.Ok(new { message = "Order status updated." })
        : Results.BadRequest(new { message = "Invalid status or order not found." });
});

app.Run();

static string GetUserId(ClaimsPrincipal user)
{
    return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.Identity?.Name ?? "anonymous";
}

public record UpdateOrderStatusRequest(string Status);

