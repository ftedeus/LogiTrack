# LogiTrack

LogiTrack is an ASP.NET Core Web API for inventory and order management, featuring authentication, role-based authorization, and caching. It uses Entity Framework Core with SQLite and supports JWT authentication.

## Features

- Inventory CRUD (Create, Read, Update, Delete)
- Order CRUD and summaries
- User authentication and registration (JWT)
- Role-based authorization (Manager, etc.)
- In-memory caching for performance
- Entity Framework Core with SQLite
- Swagger/OpenAPI documentation

## Project Structure

```
Controllers/         # API controllers (Inventory, Order, Auth, RoleAdmin)
Data/                # Data access, repositories, DbContext, seeders
Models/              # Entity and DTO classes
Migrations/          # EF Core migrations
Program.cs           # Main entry point and configuration
appsettings.json     # Configuration (connection strings, JWT, etc.)
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQLite](https://www.sqlite.org/download.html) (optional, for DB inspection)

### Setup

1. **Clone the repository:**
   ```sh
   git clone <your-repo-url>
   cd <repo-folder>
   ```

2. **Restore dependencies:**
   ```sh
   dotnet restore
   ```

3. **Apply migrations and create the database:**
   ```sh
   dotnet ef database update
   ```

4. **Run the application:**
   ```sh
   dotnet run
   ```

5. **Access the API docs:**
   - Navigate to `https://localhost:5001/swagger` in your browser.

## API Endpoints

- `GET /api/v1/inventory` - List inventory items
- `POST /api/v1/inventory` - Add inventory item (Manager role)
- `GET /api/v1/orders` - List orders
- `POST /api/v1/orders` - Create order
- `POST /api/auth/register` - Register user
- `POST /api/auth/login` - Login and get JWT

## Configuration

Edit `appsettings.json` for:

- Database connection string
- JWT settings (Issuer, Audience, Key)

## Development

- Use `dotnet watch run` for hot reload.
- Update models and run `dotnet ef migrations add <Name>` to create new migrations.

## License

MIT License

---

**Note:** For demo/testing, sample data can be seeded by uncommenting the seeder code in `Program.cs`.
