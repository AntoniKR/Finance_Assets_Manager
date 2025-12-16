using System.ComponentModel.DataAnnotations;

namespace FinancialAssetsApp.Models
{
    public class Crypto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите тикер криптовалюты")]
        [StringLength(10, MinimumLength = 1)]
        public string Ticker { get; set; } = string.Empty;  // Тикер криптовалюты 

        public string? NameCrypto { get; set; }     // Название криптовалюты

        [Required(ErrorMessage = "Введите цену больше 0,0000001")]
        [Range(0.0000001, double.MaxValue, ErrorMessage = "Цена меньше 0!")] //Ограничение на ввод
        public decimal Price { get; set; }           // Цена акции

        [Required(ErrorMessage = "Введите количество криптовалюты")]        
        [Range(0.000001, double.MaxValue, ErrorMessage = "Количество криптовалюты должно быть больше 0")] //Ограничение на ввод
        [RegularExpression(@"^\d+(\.\d+)?$", ErrorMessage = "Введите только цифры")]
        public decimal AmountCrypto { get; set; }        // Количество криптовалюты
        public decimal SumCrypto { get; set; }        // Стоимость криптовалюты
        public decimal SumCryptoToRuble { get; set; }        // Стоимость криптовалюты в рублях
        public DateTime DateAddStock { get; set; } = DateTime.UtcNow;    // Время обновления

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
