using System.ComponentModel.DataAnnotations;

namespace FinancialAssetsApp.Models
{
    public class Transport
    {
        public int Id { get; set; }                 // Идентификация

        [Required(ErrorMessage = "Введите тип движимого имущества")]
        public string TypeTransport { get; set; } = string.Empty;  // Тип движимого имущества
        [Required(ErrorMessage = "Введите название своего транспорта")]
        public string NameTransport { get; set; } = "Транспорт...";     // Название транспорта
                                                       // 
        [Required(ErrorMessage = "Введите цену больше 10")]
        [Range(10, double.MaxValue, ErrorMessage = "Цена меньше 0!")] //Ограничение на ввод
        public decimal Price { get; set; }           // Цена движимого имущества
        public decimal? YearOfTransport { get; set; } = DateTime.UtcNow.Year;        // Год выпуска движимого имущества
        public DateTime DateAddStock { get; set; } = DateTime.UtcNow;    // Время обновления

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
