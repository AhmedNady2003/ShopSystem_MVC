namespace ShopWebMVC.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public Card Card { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal DebtValue { get; set; } = 0;  // Monetary debt value
        public decimal SupportAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DifferenceAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal Paide { get; set; }
        public decimal BreadAmount { get; set; } = 0;

        // Delivery properties
        public bool IsDelivered { get; set; } = false;// Indicates if the invoice is marked for delivery
        public int? DeliveryId { get; set; } // Foreign key for associated delivery
       
        public bool IsInDeliveryList { get; set; } = false; // Indicates if the invoice is in the delivery list
        public bool IsSpent => Card != null && Card.HasInvoice && RemainingAmount == 0;


        // Navigation property for the invoice items
        public ICollection<InvoiceItem> InvoiceItems { get; set; }
    }
}
