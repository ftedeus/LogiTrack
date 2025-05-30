namespace LogiTrack.Models
{
    public class InventoryItem
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Location { get; set; }

        public string DisplayInfo()
        {
            return $"Item: {Name} | Quantity: {Quantity} | Location: {Location}";
        }
    }
}
