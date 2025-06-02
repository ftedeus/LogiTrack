using Microsoft.EntityFrameworkCore;
using LogiTrack.Models;
using Microsoft.Extensions.Caching.Memory; // Add this
using System.Diagnostics; // Add this

namespace LogiTrack.Data
{
    public interface IInventoryRepository
    {
        Task<bool> ExistsAsync(int id);
        Task<InventoryItem> AddAsync(InventoryItem item);
        Task<List<InventoryItem>> GetAllAsync();
        Task<InventoryItem?> GetByIdAsync(int id);
        Task UpdateAsync(InventoryItem item);
        Task<bool> DeleteAsync(int id);
        Task<OrderSummaryDto?> GetOrderSummaryAsync(int orderId);
        Task<List<OrderSummaryDto>> GetAllOrderSummariesAsync();
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order> AddOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int id);
    }

    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;

        public InventoryRepository(AppDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<bool> ExistsAsync(int id) =>
            await _db.InventoryItems.AsNoTracking().AnyAsync(i => i.Id == id);

        public async Task<InventoryItem> AddAsync(InventoryItem item)
        {
            _db.InventoryItems.Add(item);
            await _db.SaveChangesAsync();
            _cache.Remove("inventory_all"); // Invalidate cache after add
            return item;
        }

        public async Task<List<InventoryItem>> GetAllAsync()
        {
            var sw = Stopwatch.StartNew();

            if (_cache.TryGetValue("inventory_all", out List<InventoryItem> cachedItems))
            {
                sw.Stop();
                Console.WriteLine($"Returned InventoryItems from cache. Count: {cachedItems.Count} Records. Elapsed: {sw.ElapsedMilliseconds} ms");
                return cachedItems;
            }

            var items = await _db.InventoryItems
                .AsNoTracking()
                .ToListAsync();
            _cache.Set("inventory_all", items, TimeSpan.FromMinutes(5));
            sw.Stop();
            Console.WriteLine($"Returned InventoryItems from database. Elapsed: {sw.ElapsedMilliseconds} ms");
            return items;
        }

        public async Task<InventoryItem?> GetByIdAsync(int id)
        {
            return await _db.InventoryItems
                .AsNoTracking()
                .Where(i => i.Id == id)
                .Select(i => new InventoryItem
                {
                    Id = i.Id,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    // ...add other needed properties...
                })
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(InventoryItem item)
        {
            _db.InventoryItems.Update(item);
            await _db.SaveChangesAsync();
            _cache.Remove("inventory_all"); // Invalidate cache after update
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _db.InventoryItems.FindAsync(id);
            if (item == null)
                return false;

            _db.InventoryItems.Remove(item);
            await _db.SaveChangesAsync();
            _cache.Remove("inventory_all"); // Invalidate cache after delete
            return true;
        }

        // --- Order CRUD methods ---

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .ThenInclude(oi => oi.InventoryItem)
                .Select(o => new Order
                {
                    OrderId = o.OrderId,
                    CustomerName = o.CustomerName,
                    OrderPlaced = o.OrderPlaced,
                    Items = o.Items.Select(oi => new OrderItem
                    {
                        OrderId = oi.OrderId,
                        ItemId = oi.ItemId,
                        Quantity = oi.Quantity,
                        InventoryItem = new InventoryItem
                        {
                            Id = oi.InventoryItem.Id,
                            Name = oi.InventoryItem.Name,
                            Quantity = oi.InventoryItem.Quantity
                        }
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _db.Orders
                .AsNoTracking()
                .Where(o => o.OrderId == id)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.InventoryItem)
                .Select(o => new Order
                {
                    OrderId = o.OrderId,
                    CustomerName = o.CustomerName,
                    OrderPlaced = o.OrderPlaced,
                    Items = o.Items.Select(oi => new OrderItem
                    {
                        OrderId = oi.OrderId,
                        ItemId = oi.ItemId,
                        Quantity = oi.Quantity,
                        InventoryItem = new InventoryItem
                        {
                            Id = oi.InventoryItem.Id,
                            Name = oi.InventoryItem.Name,
                            Quantity = oi.InventoryItem.Quantity
                        }
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Order> AddOrderAsync(Order order)
        {
            // Only keep required fields for Order and OrderItem
            var newOrder = new Order
            {
                OrderId = order.OrderId,
                CustomerName = order.CustomerName,
                OrderPlaced = order.OrderPlaced,
                Items = order.Items?.Select(oi => new OrderItem
                {
                    OrderId = oi.OrderId,
                    ItemId = oi.ItemId,
                    Quantity = oi.Quantity
                }).ToList()
            };

            _db.Orders.Add(newOrder);
            await _db.SaveChangesAsync();
            // Optionally clear cache after mutation
            _cache.Remove("order_summaries_all");
            return newOrder;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null)
                return false;

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
            // Optionally clear cache after mutation
            _cache.Remove("order_summaries_all");
            return true;
        }

        private static OrderSummaryDto ToOrderSummaryDto(Order order)
        {
            return new OrderSummaryDto
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderPlaced,
                Items = order.Items.Select(oi => new OrderItemSummaryDto
                {
                    ItemId = oi.ItemId,
                    Name = oi.InventoryItem.Name,
                    Quantity = oi.Quantity
                }).ToList()
            };
        }

        public async Task<OrderSummaryDto?> GetOrderSummaryAsync(int orderId)
        {
            var order = await _db.Orders
                .AsNoTracking()
                .Where(o => o.OrderId == orderId)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.InventoryItem)
                .Select(o => new OrderSummaryDto
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderPlaced,
                    Items = o.Items.Select(oi => new OrderItemSummaryDto
                    {
                        ItemId = oi.ItemId,
                        Name = oi.InventoryItem.Name,
                        Quantity = oi.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return order;
        }

        public async Task<List<OrderSummaryDto>> GetAllOrderSummariesAsync()
        {
            if (_cache.TryGetValue("order_summaries_all", out List<OrderSummaryDto> cachedSummaries))
            {
                Console.WriteLine("Returned OrderSummaries from cache.");
                return cachedSummaries;
            }

            var summaries = await _db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .ThenInclude(oi => oi.InventoryItem)
                .Select(o => new OrderSummaryDto
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderPlaced,
                    Items = o.Items.Select(oi => new OrderItemSummaryDto
                    {
                        ItemId = oi.ItemId,
                        Name = oi.InventoryItem.Name,
                        Quantity = oi.Quantity
                    }).ToList()
                })
                .ToListAsync();

            _cache.Set("order_summaries_all", summaries, TimeSpan.FromMinutes(5));
            Console.WriteLine("Returned OrderSummaries from database.");
            return summaries;
        }

        // Returns the number of InventoryItems in cache, or 0 if not cached
        public int GetCachedInventoryItemCount()
        {
            if (_cache.TryGetValue("inventory_all", out List<InventoryItem> cachedItems))
                return cachedItems.Count;
            return 0;
        }

        // Returns the number of OrderSummaries in cache, or 0 if not cached
        public int GetCachedOrderSummaryCount()
        {
            if (_cache.TryGetValue("order_summaries_all", out List<OrderSummaryDto> cachedSummaries))
                return cachedSummaries.Count;
            return 0;
        }
    }

    // DTOs
    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItemSummaryDto> Items { get; set; }
    }

    public class OrderItemSummaryDto
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}