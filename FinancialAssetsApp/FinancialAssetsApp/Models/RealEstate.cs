using System.ComponentModel.DataAnnotations;

namespace FinancialAssetsApp.Models
{
    public class RealEstate
    {
        public int Id { get; set; }                 // Идентификация

        [Required(ErrorMessage = "Введите тип недвижимости")]
        public string TypeEstate { get; set; } = string.Empty;  // Тип недвижимости

        [Required(ErrorMessage = "Введите расположение недвижимости")]
        public string CityEstate { get; set; } = string.Empty;  // Город расположения недвижимости

        [Required(ErrorMessage = "Введите цену больше 10")]
        [Range(10, double.MaxValue, ErrorMessage = "Цена меньше 0!")] //Ограничение на ввод
        public decimal Price { get; set; }           // Цена недвижимости

        [Required(ErrorMessage = "Введите количество недвижимости")]
        [Range(1, double.MaxValue, ErrorMessage = "Количество недвижимости должно быть больше 0")] //Ограничение на ввод
        public int AmountEstate { get; set; }        // Количество недвижимости
        public decimal SumEstate { get; set; }        // Стоимость недвижимости

        public DateTime DateAddStock { get; set; } = DateTime.UtcNow;    // Время обновления

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
