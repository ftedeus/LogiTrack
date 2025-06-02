namespace LogiTrack.Models
{
    public class OrderItem
    {
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }

        public InventoryItem InventoryItem { get; set; }

        public override string ToString()
        {
            return $"Order ID: {OrderId}, Item ID: {ItemId}, Quantity: {Quantity}";
        }
    }
}