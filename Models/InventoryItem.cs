public class InventoryItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Quantity { get; set; }
    public string? Location { get; set; }
    public decimal Price { get; set; }

    public override string ToString()
    {
        return $"{Name} - Quantity: {Quantity}, Price: {Price:C}";
    }

    public string DisplayInfo()
    {
        return $"{Name} - Quantity: {Quantity}, Location: {Location}, Price: {Price:C}";
    }   
}