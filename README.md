# WebShop

## Simple role model (easy to understand)

The app currently has 3 access levels:

- Guest: not logged in, can view public products.
- User: logged in user, can use user endpoints (cart).
- Admin: logged in user with Admin role, can manage products.

This is intentionally simple and can be extended later with full policy-based authorization.

Product data is now stored in SQL Server via Entity Framework Core.

## Identity seed for local development

On startup, the app creates roles `Admin` and `User` if they do not exist.
It also seeds one admin account using `AdminSeed` values from appsettings.
When an authenticated non-admin user makes requests, the app ensures the user gets role `User`.
If there are no products in database, a few starter products are seeded automatically.

Default local admin:

- Email: `admin@webshop.local`
- Password: `Admin123!`

## API endpoints

### Guest

- `GET /api/me`
- `GET /api/public/products`
- `GET /api/public/products/{id}`

### Logged in user

- `GET /api/user/cart`
- `POST /api/user/cart/{productId}`
- `POST /api/user/orders`
- `GET /api/user/orders`

### Admin only

- `POST /api/admin/products`
- `PUT /api/admin/products/{id}`
- `DELETE /api/admin/products/{id}`
- `GET /api/admin/orders`

## Razor Pages

- `/User/Orders` (logged in user)
- `/Admin/Orders` (admin only)

## Future plan

- Add default `User` role assignment at registration.
- Move product and cart from in-memory service to database tables.
- Add order endpoints and admin order management.
- Add policy-based authorization for finer control.