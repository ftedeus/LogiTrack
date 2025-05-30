using System;
using System.Collections.Generic;

namespace LogiTrack.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime DatePlaced { get; set; }
        public List<InventoryItem> Items { get; set; }
    }
}