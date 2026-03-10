using System.ComponentModel.DataAnnotations;

namespace FinancialAssetsApp.Models
{
    public class Currency
    {
        public int Id { get; set; }                 // Record identifier
        [Required(ErrorMessage = "Enter the currency name")]
        public string NameCurrency { get; set; } = string.Empty;  // Currency name
        public string CharCode { get; set; } = "RUB";     // Currency code
        [Required(ErrorMessage = "Enter a price greater than 0.000000001")]
        [Range(0.000000001, double.MaxValue, ErrorMessage = "Price must be greater than 0")] // Input validation
        public decimal Price { get; set; }           // Purchase price
        [Required(ErrorMessage = "Enter the currency amount")]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")] // Input validation
        public decimal AmountCurrency { get; set; }        // Amount of currency
        public decimal SumCurrencyToRuble { get; set; }        // Total value in RUB
        public DateTime DateAddStock { get; set; } = DateTime.UtcNow;    // Last updated timestamp
        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
