using System.ComponentModel.DataAnnotations;

namespace FinancialAssetsApp.Models
{
    public class Currency
    {
        public int Id { get; set; }                 // Идентификация

        [Required(ErrorMessage = "Введите название валюты")]
        
        public string NameCurrency { get; set; } = string.Empty;  // Название валюты 

        public string CharCode { get; set; } = "RUB";     // Код валюты

        [Required(ErrorMessage = "Введите цену больше 0,000000001")]
        [Range(0.000000001, double.MaxValue, ErrorMessage = "Цена меньше 0!")] //Ограничение на ввод
        public decimal Price { get; set; }           // Цена валюты

        [Required(ErrorMessage = "Введите сумму валюты")]
        [Range(1, double.MaxValue, ErrorMessage = "Сумма валюты должна быть больше 0")] //Ограничение на ввод
        public decimal AmountCurrency { get; set; }        // Сумма валюты
        public decimal SumCurrencyToRuble { get; set; }        // Сумма валюты в рублях
        public DateTime DateAddStock { get; set; } = DateTime.UtcNow;    // Время обновления

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
