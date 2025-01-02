
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopWebMVC.Models
{
    public class Card
    {
        
       
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set ; } // Unique identifier for the card
        public string FirstName { get; set; } // First name of the cardholder
        public string? SecondName { get; set; } // Second name of the cardholder
        public string? ThirdName { get; set; } // Third name of the cardholder (if applicable)
        public string? FourthName { get; set; } // Fourth name of the cardholder (if applicable)

        public string? PhoneNumber { get; set; } // Phone number of the cardholder (if applicable)
        public string? Address { get; set; } //  address 

        public int NumberOfIndividuals { get; set; } // Number of individuals in the card
        public string? Notes { get; set; } // Additional notes about the card
        public bool IsStruck { get; set; } = false; // Indicates if the card has been struck this month
         
        public Debt Debt { get; set; } // Reference to the associated debt
        public List<Invoice> Invoices { get; set; } // List of invoices associated with the card
        public int? BakeryId { get; set; } // Foreign key to the associated bakery (if any)
        public Bakery Bakery { get; set; } // Navigation property to the bakery
        public bool IsInQueue { get; set; } = false; // Indicates if the invoice is in the delivery queue                                              
        public bool HasInvoice => Invoices.Any(i => i.InvoiceDate.Month == DateTime.Now.Month && i.InvoiceDate.Year == DateTime.Now.Year) ;
        // Property to check if an invoice was created this month
    }
}
