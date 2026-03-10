using System.ComponentModel.DataAnnotations;

namespace FinancialAssetsApp.Models
{
    public class LoginModel // Model for login form
    {
        [Required]
        public string Username {  get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
