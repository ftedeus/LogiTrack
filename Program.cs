using LogiTrack.Models;
using Microsoft.OpenApi.Models; // Optional, for OpenApi info
using Microsoft.AspNetCore.Builder; // Ensure this is present
using Microsoft.Extensions.DependencyInjection; // Ensure this is present

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var inventoryItems = new List<InventoryItem>
{
    new InventoryItem { ItemId = 1, Name = "Widget A", Quantity = 100, Location = "Warehouse 1" },
    new InventoryItem { ItemId = 2, Name = "Widget B", Quantity = 50, Location = "Warehouse 2" },
    new InventoryItem { ItemId = 3, Name = "Widget C", Quantity = 200, Location = "Warehouse 3" }
};

// Configure the HTTP request pipeline.
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

v1.MapGet("/inventory", () => inventoryItems)
    .WithName("GetInventory")
    .WithOpenApi();

v1.MapGet("/inventory/{id}", (int id) =>
{
    var item = inventoryItems.FirstOrDefault(i => i.ItemId == id);
    if (item == null)
    {
        return Results.NotFound($"Looking for ItemId: {id},  not found: ");
    }
    return Results.Ok(item.DisplayInfo());
});

v1.MapPost("/inventory", (InventoryItem item) =>
{
    // Prevent duplicates by ItemId
    if (inventoryItems.Any(i => i.ItemId == item.ItemId))
    {
        return Results.Conflict($"An item with ItemId {item.ItemId} already exists.");
    }
    inventoryItems.Add(item);
    return Results.Created($"/InventoryItems/{inventoryItems.Count - 1}", item);
});

v1.MapPut("/inventory/{id}", (int id, InventoryItem item) =>
{
    var existingItem = inventoryItems.FirstOrDefault(i => i.ItemId == id);
    if (existingItem == null)
    {
        return Results.NotFound($"Item {id}  Not found.");
    }
    // Prevent changing ItemId to a duplicate
    if (item.ItemId != id && inventoryItems.Any(i => i.ItemId == item.ItemId))
    {
        return Results.Conflict($"An item with ItemId {item.ItemId} already exists.");
    }
    // Update properties
    existingItem.ItemId = item.ItemId;
    existingItem.Name = item.Name;
    existingItem.Quantity = item.Quantity;
    existingItem.Location = item.Location;
    return Results.Ok(existingItem);
});

v1.MapDelete("/inventory/{id}", (int id) =>
{
    var item = inventoryItems.FirstOrDefault(i => i.ItemId == id);
    if (item == null)
    {
        return Results.NotFound($"Item {id} not found.");
    }
    // display item info before deletion
    var info = item.DisplayInfo();
    inventoryItems.Remove(item);
    return Results.Ok($"Item deleted: {info}");
});

v1.MapGet("/inventory/OrderSummary/{id}", (int id) =>
{
    var item = inventoryItems.FirstOrDefault(i => i.ItemId == id);
    if (item == null)
    {
        return Results.NotFound($"Item {id} not found.");
    }
    return Results.Ok(item.DisplayInfo());
});

// Register v2 endpoints from a separate file
V2Endpoints.Register(v2: app.MapGroup("/api/v2"), inventoryItems);

app.Run();