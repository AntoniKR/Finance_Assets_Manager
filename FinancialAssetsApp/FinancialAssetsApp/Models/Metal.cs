using System.ComponentModel.DataAnnotations;

namespace FinancialAssetsApp.Models
{
    public class Metal
    {
        public int Id { get; set; }                 // Идентификация

        [Required(ErrorMessage = "Введите металл")]
        public string NameMetal { get; set; } = string.Empty;  // Название металла

        [Required(ErrorMessage = "Введите цену больше 10")]
        [Range(10, double.MaxValue, ErrorMessage = "Цена меньше 0!")] //Ограничение на ввод
        public decimal Price { get; set; }           // Цена металла

        [Required(ErrorMessage = "Введите количество металла")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Количество металла должно быть больше 0")] //Ограничение на ввод
        public decimal AmountMetal { get; set; }        // Количество металла
        public decimal SumMetals { get; set; }        // Стоимость металла

        public DateTime DateAddStock { get; set; } = DateTime.UtcNow;    // Время обновления

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
