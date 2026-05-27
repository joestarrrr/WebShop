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
using WebShop.Data;

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
.AddEntityFrameworkStores<ApplicationDbContext>();

// 3. Add Razor Pages support (the page-based UI framework we use).
builder.Services.AddRazorPages();

// ---------------------------------------------------------------------------
// APP PHASE — configure the HTTP request pipeline (middleware order matters!)
// ---------------------------------------------------------------------------

var app = builder.Build();

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

// Enable authorization (checking if the user has permission to access a page).
app.UseAuthorization();

// Map Razor Pages routes.
// Razor Pages routing works by convention:
//   Pages/Index.cshtml          → /
//   Pages/Privacy.cshtml        → /Privacy
//   Pages/Products/List.cshtml  → /Products/List
// No explicit route configuration is needed for simple pages.
app.MapRazorPages();

app.Run();

