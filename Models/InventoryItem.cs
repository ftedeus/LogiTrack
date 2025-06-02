namespace LogiTrack.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }

        public string DisplayInfo() => $"{Id}: {Name}";
    }
}