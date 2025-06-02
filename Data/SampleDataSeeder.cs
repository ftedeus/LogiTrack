using System;
using LogiTrack.Models;

public static class SampleDataSeeder
{
    public static void Seed(AppDbContext db)
    {
        // Example usage of InventoryItem
        var item1 = new InventoryItem
        {
            Name = "Laptop",
            Quantity = 10,
            Location = "Warehouse A",
            Price = 999.99m
        };
        var item2 = new InventoryItem
        {
            Name = "Mouse",
            Quantity = 50,
            Location = "Warehouse B",
            Price = 19.99m
        };
        db.InventoryItems.AddRange(item1, item2);
        db.SaveChanges();
        Console.WriteLine(item1);
        Console.WriteLine(item2);

        // Example usage of Order with multiple OrderItems
        var order = new Order
        {
            CustomerName = "John Doe",
            OrderPlaced = DateTime.Now
        };
        db.Orders.Add(order);
        db.SaveChanges(); // OrderId is now set

        var orderItem1 = new OrderItem { OrderId = order.OrderId, ItemId = item1.Id, Quantity = 2 };
        var orderItem2 = new OrderItem { OrderId = order.OrderId, ItemId = item2.Id, Quantity = 5 };
        order.Items.Add(orderItem1);
        order.Items.Add(orderItem2);
        db.OrderItems.AddRange(orderItem1, orderItem2);
        db.SaveChanges();
    }
}