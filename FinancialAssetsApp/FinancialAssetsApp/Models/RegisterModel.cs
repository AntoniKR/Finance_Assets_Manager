using System.ComponentModel.DataAnnotations;

namespace FinancialAssetsApp.Models
{
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }    // Username

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }    // Password

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Invalid password")] // Confirmed password
        public string ConfirmPassword { get; set; }
    }
}
