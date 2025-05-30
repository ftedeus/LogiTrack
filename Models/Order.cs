using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required]
        public DateTime DatePlaced { get; set; }

        [Required]
        public List<InventoryItem> Items { get; set; }
    }
}