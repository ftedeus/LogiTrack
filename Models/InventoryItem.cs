using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    public class InventoryItem
    {
        [Key]
        public int ItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        public string DisplayInfo()
        {
            return $"Item: {Name} | Quantity: {Quantity} | Location: {Location}";
        }
    }
}
