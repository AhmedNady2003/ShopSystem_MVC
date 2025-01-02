using System.ComponentModel.DataAnnotations.Schema;

namespace ShopWebMVC.Models
{
    public class Item
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; } // Unique identifier for the item
        public string Name { get; set; } // Name of the item
        public decimal Price { get; set; } // Price of the item
        public List<InvoiceItem> InvoiceItems { get; set; }
        public List<DebtItem> DebtItems { get; set; }
    }
}
