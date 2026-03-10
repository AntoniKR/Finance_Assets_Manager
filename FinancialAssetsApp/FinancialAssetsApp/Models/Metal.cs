using System.ComponentModel.DataAnnotations;

namespace FinancialAssetsApp.Models
{
    public class Metal
    {
        public int Id { get; set; }                 // Record identifier
        [Required(ErrorMessage = "Enter the metal name")]
        public string NameMetal { get; set; } = string.Empty;  // Metal name
        [Required(ErrorMessage = "Enter a price greater than 10")]
        [Range(10, double.MaxValue, ErrorMessage = "Price must be greater than 0")] // Input validation
        public decimal Price { get; set; }           // Purchase price
        [Required(ErrorMessage = "Enter the metal amount")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")] // Input validation
        public decimal AmountMetal { get; set; }        // Amount of metal (grams)
        public decimal SumMetals { get; set; }        // Total metal value
        public DateTime DateAddStock { get; set; } = DateTime.UtcNow;    // Last updated timestamp
        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
