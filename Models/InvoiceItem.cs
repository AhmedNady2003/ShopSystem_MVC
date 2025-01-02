namespace ShopWebMVC.Models
{
    public class InvoiceItem
    {
        
        public int InvoiceId { get; set; } // Foreign key for the associated invoice
        public Invoice Invoice { get; set; } // Navigation property for the associated invoice
        public int ItemId { get; set; } // Foreign key for the associated item
        public Item Item { get; set; } // Navigation property for the associated item
        public int Quantity { get; set; } // Quantity of the item
        public decimal Price { get; set; } 
        
    }
}
