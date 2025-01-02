using System.ComponentModel.DataAnnotations.Schema;

namespace ShopWebMVC.Models
{
    public class Debt
    {
        
        public int Id { get; set; } // Unique identifier for the debt
        public int CardId { get; set; } // Foreign key to the associated card

        public decimal MonetaryDebt { get; set; }  // Monetary debt value
        
        public Card Card { get; set; } // Navigation property to the card
        

        public bool IsSettled { get; set; } = false;

        public ICollection<DebtItem>? DebtItems { get; set; }  // العلاقة مع الأصناف

        
    }
}
