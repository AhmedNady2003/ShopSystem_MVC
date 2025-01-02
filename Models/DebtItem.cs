namespace ShopWebMVC.Models
{
    public class DebtItem
    {
        public int DebtId { get; set; }
        public Debt Debt { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }

        public int Quantity { get; set; }
    }
}
