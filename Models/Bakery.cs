using System.ComponentModel.DataAnnotations.Schema;

namespace ShopWebMVC.Models
{
    public class Bakery
    {
        
        
        
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; } // Unique identifier for the bakery
        public string Name { get; set; } // Name of the bakery
        public List<Card> Cards { get; set; } // List of cards associated with this bakery
    }
}
