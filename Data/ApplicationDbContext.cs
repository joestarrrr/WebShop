using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebShop.Models;

namespace WebShop.Data;

/// <summary>
/// ApplicationDbContext is the "bridge" between your C# code and the database.
/// 
/// It inherits from IdentityDbContext, which already includes all the tables
/// that ASP.NET Identity needs (users, roles, logins, etc.).
/// 
/// How it works:
///   - Every DbSet&lt;T&gt; property here becomes a table in the database.
///   - You use it to query or save data: _context.Products.ToList(), etc.
///   - Entity Framework Core reads this class to generate SQL migrations.
/// 
/// Later you will add DbSet properties here for products, orders, etc.
/// </summary>
public class ApplicationDbContext : IdentityDbContext
{
    // The constructor receives DbContextOptions from Program.cs (where we
    // register the connection string). We just pass it to the base class.
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<CartItem> CartItems { get; set; } = default!;
    public DbSet<Order> Orders { get; set; } = default!;
    public DbSet<OrderItem> OrderItems { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        builder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        builder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        builder.Entity<CartItem>()
            .Property(ci => ci.UnitPrice)
            .HasPrecision(18, 2);
    }
}
