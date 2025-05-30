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
    }
}
