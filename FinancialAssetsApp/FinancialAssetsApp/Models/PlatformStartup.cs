using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialAssetsApp.Models
{
    public class PlatformStartup
    {
        public int Id { get; set; }                 // Record identifier
        [Required(ErrorMessage = "Enter the platform name")]
        public string NamePlatform { get; set; } = string.Empty;  // Platform name
        public int? AmountCompanies { get; set; }       // Number of invested companies
        public decimal? SumOfStartups { get; set; }      // Total value of startup shares
        public DateTime DateAddStock { get; set; } = DateTime.UtcNow;    // Last updated timestamp
        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
