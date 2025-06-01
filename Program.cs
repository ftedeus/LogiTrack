using Microsoft.OpenApi.Models; // Optional, for OpenApi info
using Microsoft.AspNetCore.Builder; // Ensure this is present
using Microsoft.Extensions.DependencyInjection; // Ensure this is present
using Microsoft.EntityFrameworkCore; // Add this
using Microsoft.Extensions.Configuration; // Add this

var builder = WebApplication.CreateBuilder(args);

// Add this to read connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Use AppDbContext and SQLite with connection string from config
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register InventoryRepository in DI
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();


var app = builder.Build();

// Seed sample data using DI scope
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     SampleDataSeeder.Seed(db);
//     Console.WriteLine("Sample data inserted.");
// }

 

// // Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var v1 = app.MapGroup("/api/v1");

v1.MapGet("/", () => "Welcome to LogiTrack API v1!")
    .WithName("GetRoot")
    .WithOpenApi();


v1.MapGet("/inventory", async (IInventoryRepository repo) =>
    await repo.GetAllAsync())
    .WithName("GetInventory")
    .WithOpenApi();

v1.MapGet("/inventory/{id}", async (IInventoryRepository repo, int id) =>
{
    var item = await repo.GetByIdAsync(id);
    if (item == null)
    {
        return Results.NotFound($"Looking for ItemId: {id},  not found: ");
    }
    return Results.Ok(item.DisplayInfo());
});

v1.MapPost("/inventory", async (IInventoryRepository repo, InventoryItem item) =>
{
    // Prevent duplicates by Id
    if (await repo.ExistsAsync(item.Id))
    {
        return Results.Conflict($"An item with Id {item.Id} already exists.");
    }
    await repo.AddAsync(item);
    return Results.Created($"/api/v1/inventory/{item.Id}", item);
});

v1.MapPut("/inventory/{id}", async (IInventoryRepository repo, int id, InventoryItem item) =>
{
    var existingItem = await repo.GetByIdAsync(id);
    if (existingItem == null)
    {
        return Results.NotFound($"Item {id} Not found.");
    }
    // Prevent changing Id to a duplicate
    if (item.Id != id && await repo.ExistsAsync(item.Id))
    {
        return Results.Conflict($"An item with Id {item.Id} already exists.");
    }
    // Update properties
    existingItem.Name = item.Name;
    existingItem.Quantity = item.Quantity;
    existingItem.Location = item.Location;
    await repo.UpdateAsync(existingItem);
    return Results.Ok(existingItem);
});

v1.MapDelete("/inventory/{id}", async (IInventoryRepository repo, int id) =>
{
    var item = await repo.GetByIdAsync(id);
    if (item == null)
    {
        return Results.NotFound($"Item {id} not found.");
    }
    var info = item.DisplayInfo();
    await repo.DeleteAsync(item);
    return Results.Ok($"Item deleted: {info}");
});

v1.MapGet("/inventory/OrderSummary/{id}", async (IInventoryRepository repo, int id) =>
{
    var summary = await repo.GetOrderSummaryAsync(id);
    if (summary == null)
        return Results.NotFound($"Order {id} not found.");
    return Results.Ok(summary);
})
.WithName("GetOrderSummary")
.WithOpenApi();

v1.MapGet("/inventory/OrderSummary", async (IInventoryRepository repo) =>
{
    var summaries = await repo.GetAllOrderSummariesAsync();
    return Results.Ok(summaries);
})
.WithName("GetAllOrderSummaries")
.WithOpenApi();

// Register v2 endpoints from a separate file
//V2Endpoints.Register(v2: app.MapGroup("/api/v2"), inventoryItems);

app.Run();