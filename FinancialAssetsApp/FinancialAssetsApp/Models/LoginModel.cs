using System.ComponentModel.DataAnnotations;

namespace FinancialAssetsApp.Models
{
    public class LoginModel //Модель для формы логина
    {
        [Required]
        public string Username {  get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
