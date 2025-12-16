using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialAssetsApp.Models
{
    public class Stock
    {
        public int Id { get; set; }                 // Идентификация

        [Required(ErrorMessage = "Введите тикер акции")]
        [StringLength(4, MinimumLength = 1)]
        public string Ticker { get; set; } = string.Empty;  // Тикер акции 
        public string? NameCompany { get; set; }     // Название компании

        [Required(ErrorMessage = "Введите цену больше 0,01")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена меньше 0!")] //Ограничение на ввод
        public decimal Price { get; set; }           // Цена акции

        [Required(ErrorMessage = "Введите количество акции")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество акций должно быть больше 0")] //Ограничение на ввод
        public int AmountStock { get; set; }        // Количество акций
        public decimal SumStocks { get; set; }        // Стоимость акций
        public DateTime DateAddStock { get; set; } = DateTime.UtcNow;    // Время обновления
        [Required]
        public int UserId { get; set; }
        public User? User { get; set; } // Как навигационное свойство, чтобы обращаться к связанным данным не вручную
    }
}
