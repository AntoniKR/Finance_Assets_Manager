using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialAssetsApp.Models
{
    public class PlatformStartup
    {
        public int Id { get; set; }                 // Идентификация

        [Required(ErrorMessage = "Введите название платформы")]
        public string NamePlatform { get; set; } = string.Empty;  // Название платформы
        public int? AmountCompanies { get; set; }       // Количество проинвестированных компаний
        public decimal? SumOfStartups { get; set; }      // Стоимость акций
        public DateTime DateAddStock { get; set; } = DateTime.UtcNow;    // Время обновления

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

    }
}
