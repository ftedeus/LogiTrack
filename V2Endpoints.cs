using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using LogiTrack.Models;

public static class V2Endpoints
{
    public static void Register(RouteGroupBuilder v2, List<InventoryItem> inventoryItems)
    {
        v2.MapGet("/", () => "Welcome to LogiTrack API v2!")
            .WithName("GetRootV2")
            .WithOpenApi();

        v2.MapGet("/inventory", () => inventoryItems)
            .WithName("GetInventoryV2")
            .WithOpenApi();

        v2.MapGet("/inventory/{id}", (int id) =>
        {
            var item = inventoryItems.FirstOrDefault(i => i.ItemId == id);
            if (item == null)
            {
                return Results.NotFound($"[v2] Looking for ItemId: {id}, not found.");
            }
            return Results.Ok(item.DisplayInfo());
        });

        v2.MapPost("/inventory", (InventoryItem item) =>
        {
            if (inventoryItems.Any(i => i.ItemId == item.ItemId))
            {
                return Results.Conflict($"[v2] An item with ItemId {item.ItemId} already exists.");
            }
            inventoryItems.Add(item);
            return Results.Created($"/InventoryItems/{inventoryItems.Count - 1}", item);
        });

        v2.MapPut("/inventory/{id}", (int id, InventoryItem item) =>
        {
            var existingItem = inventoryItems.FirstOrDefault(i => i.ItemId == id);
            if (existingItem == null)
            {
                return Results.NotFound($"[v2] Item {id} Not found.");
            }
            if (item.ItemId != id && inventoryItems.Any(i => i.ItemId == item.ItemId))
            {
                return Results.Conflict($"[v2] An item with ItemId {item.ItemId} already exists.");
            }
            existingItem.ItemId = item.ItemId;
            existingItem.Name = item.Name;
            existingItem.Quantity = item.Quantity;
            existingItem.Location = item.Location;
            return Results.Ok(existingItem);
        });

        v2.MapDelete("/inventory/{id}", (int id) =>
        {
            var item = inventoryItems.FirstOrDefault(i => i.ItemId == id);
            if (item == null)
            {
                return Results.NotFound($"[v2] Item {id} not found.");
            }
            var info = item.DisplayInfo();
            inventoryItems.Remove(item);
            return Results.Ok($"[v2] Item deleted: {info}");
        });

        v2.MapGet("/inventory/OrderSummary/{id}", (int id) =>
        {
            var item = inventoryItems.FirstOrDefault(i => i.ItemId == id);
            if (item == null)
            {
                return Results.NotFound($"[v2] Item {id} not found.");
            }
            return Results.Ok(item.DisplayInfo());
        });

        // In-memory orders list (for demo; in real apps, use DI/service)
        var orders = new List<Order>
        {
            new Order
            {
                OrderId = 1001,
                CustomerName = "Samir",
                DatePlaced = new DateTime(2025, 4, 5),
                Items = new List<InventoryItem> { inventoryItems.FirstOrDefault() }
            },
            new Order
            {
                OrderId = 1002,
                CustomerName = "Alex",
                DatePlaced = new DateTime(2025, 5, 1),
                Items = inventoryItems.Take(2).ToList() // first two items
            }
        };

        // Get all orders
        v2.MapGet("/orders", () => orders)
            .WithName("GetAllOrdersV2");

        // Get one order by id
        v2.MapGet("/orders/{orderId}", (int orderId) =>
        {
            var order = orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return Results.NotFound($"[v2] Order {orderId} not found.");
            return Results.Ok(order);
        }).WithName("GetOrderV2");

        // Post one order
        v2.MapPost("/orders", (Order order) =>
        {
            if (orders.Any(o => o.OrderId == order.OrderId))
                return Results.Conflict($"[v2] Order {order.OrderId} already exists.");
            orders.Add(order);
            return Results.Created($"/orders/{order.OrderId}", order);
        }).WithName("PostOrderV2");

        // Delete one order
        v2.MapDelete("/orders/{orderId}", (int orderId) =>
        {
            var order = orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return Results.NotFound($"[v2] Order {orderId} not found.");
            orders.Remove(order);
            return Results.Ok($"[v2] Order {orderId} deleted.");
        }).WithName("DeleteOrderV2");
    }
}
