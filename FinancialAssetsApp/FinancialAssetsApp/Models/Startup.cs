using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialAssetsApp.Models
{
    public class Startup
    {
        public int Id { get; set; }                 // Record identifier
        [Required(ErrorMessage = "Enter the company name")]
        [StringLength(20, MinimumLength = 1)]
        public string NameCompany { get; set; }     // Company name
        [Required(ErrorMessage = "Enter the platform name")]
        public string NamePlatform { get; set; } = string.Empty;  // Platform name
        [Required(ErrorMessage = "Enter a price greater than 0.01")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")] // Input validation
        public decimal Price { get; set; }           // Share price
        [Required(ErrorMessage = "Enter the number of shares")]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")] // Input validation
        public int AmountStock { get; set; }        // Number of shares
        public decimal SumStocks { get; set; }        // Total value of shares
        public DateTime DateAddStock { get; set; } = DateTime.UtcNow;    // Last updated timestamp
        [Required]  // Bind to a specific user
        public int UserId { get; set; }
        public User? User { get; set; }
        [Required]  // Bind to a specific platform
        public int PlatformStartupId { get; set; }
        public PlatformStartup? PlatformStartup { get; set; }
    }
}
