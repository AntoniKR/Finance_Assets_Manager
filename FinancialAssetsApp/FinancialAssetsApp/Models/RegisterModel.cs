using System.ComponentModel.DataAnnotations;

namespace FinancialAssetsApp.Models
{
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage ="пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }
}
