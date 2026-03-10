using System.ComponentModel.DataAnnotations;

namespace FinancialAssetsApp.Models
{
    public class Crypto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter a crypto ticker")]
        [StringLength(10, MinimumLength = 1)]
        public string Ticker { get; set; } = string.Empty;  // Crypto ticker symbol

        public string? NameCrypto { get; set; }     // Crypto name

        [Required(ErrorMessage = "Enter a price greater than 0.0000001")]
        [Range(0.0000001, double.MaxValue, ErrorMessage = "Price must be greater than 0")] // Input validation
        public decimal Price { get; set; }           // Purchase price

        [Required(ErrorMessage = "Enter the amount of crypto")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "Amount must be greater than 0")] // Input validation
        [RegularExpression(@"^\d+(\.\d+)?$", ErrorMessage = "Enter numbers only")]
        public decimal AmountCrypto { get; set; }        // Amount of crypto

        public decimal SumCrypto { get; set; }        // Total crypto value in USD

        public decimal SumCryptoToRuble { get; set; }        // Total crypto value in RUB

        public DateTime DateAddStock { get; set; } = DateTime.UtcNow;    // Last updated timestamp

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
