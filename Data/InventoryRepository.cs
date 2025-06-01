using Microsoft.EntityFrameworkCore;

public interface IInventoryRepository
{
    Task<bool> ExistsAsync(int id);
    Task AddAsync(InventoryItem item);
    Task<List<InventoryItem>> GetAllAsync();
    Task<InventoryItem?> GetByIdAsync(int id);
    Task UpdateAsync(InventoryItem item);
    Task DeleteAsync(InventoryItem item);
    Task<OrderSummaryDto?> GetOrderSummaryAsync(int orderId);
    Task<List<OrderSummaryDto>> GetAllOrderSummariesAsync();
}

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _db;
    public InventoryRepository(AppDbContext db) => _db = db;

    public async Task<bool> ExistsAsync(int id) =>
        await _db.InventoryItems.AnyAsync(i => i.Id == id);

    public async Task AddAsync(InventoryItem item)
    {
        _db.InventoryItems.Add(item);
        await _db.SaveChangesAsync();
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

    public async Task DeleteAsync(InventoryItem item)
    {
        _db.InventoryItems.Remove(item);
        await _db.SaveChangesAsync();
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
