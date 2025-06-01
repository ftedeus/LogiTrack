using Microsoft.EntityFrameworkCore;
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
        public InventoryRepository(AppDbContext db) => _db = db;

        public async Task<bool> ExistsAsync(int id) =>
            await _db.InventoryItems.AnyAsync(i => i.Id == id);

        public async Task<InventoryItem> AddAsync(InventoryItem item)
        {
            _db.InventoryItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task<List<InventoryItem>> GetAllAsync()
        {
            return await _db.InventoryItems.ToListAsync();
        }

        public async Task<InventoryItem?> GetByIdAsync(int id)
        {
            return await _db.InventoryItems.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task UpdateAsync(InventoryItem item)
        {
            _db.InventoryItems.Update(item);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _db.InventoryItems.FindAsync(id);
            if (item == null)
                return false;

            _db.InventoryItems.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }

        // --- Order CRUD methods ---

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.InventoryItem)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.InventoryItem)
                .FirstOrDefaultAsync(o => o.OrderId == id);
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
            return newOrder;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null)
                return false;

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
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
                .Include(o => o.Items)
                .ThenInclude(oi => oi.InventoryItem)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return null;

            return ToOrderSummaryDto(order);
        }

        public async Task<List<OrderSummaryDto>> GetAllOrderSummariesAsync()
        {
            var orders = await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.InventoryItem)
                .ToListAsync();

            return orders.Select(ToOrderSummaryDto).ToList();
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