using LogiTrack.Models;

public class Order
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; }
    public DateTime OrderPlaced { get; set; }
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();

     

    public string GetOrderSummary()
    {
        return $"Order #{OrderId} for {CustomerName} | Items: {Items.Count} | Placed: {OrderPlaced.ToShortDateString()}";
    }

    public override string ToString()
    {
        return $"Order ID: {OrderId}, Customer: {CustomerName}, Date: {OrderPlaced.ToShortDateString()}, Items: {Items.Count}";
    }
}