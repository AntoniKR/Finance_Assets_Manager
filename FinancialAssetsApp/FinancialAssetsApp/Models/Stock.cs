using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialAssetsApp.Models
{
    public class Stock
    {
        public int Id { get; set; }                 // Record identifier
        [Required(ErrorMessage = "Enter the stock ticker")]
        [StringLength(4, MinimumLength = 1)]
        public string Ticker { get; set; } = string.Empty;  // Stock ticker symbol
        public string? NameCompany { get; set; }     // Company name
        [Required(ErrorMessage = "Enter a price greater than 0.01")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")] // Input validation
        public decimal Price { get; set; }           // Share price
        [Required(ErrorMessage = "Enter the number of shares")]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")] // Input validation
        public int AmountStock { get; set; }        // Number of shares
        public decimal SumStocks { get; set; }        // Total value of shares
        public DateTime DateAddStock { get; set; } = DateTime.UtcNow;    // Last updated timestamp
        [Required]
        public int UserId { get; set; }
        public User? User { get; set; } // Navigation property for accessing related user data
    }
}
