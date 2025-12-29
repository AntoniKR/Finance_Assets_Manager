using BCrypt.Net;
using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinancialAssetsApp.Data.Service
{
    public class CurrenciesService : ICurrenciesService
    {
        private readonly FinanceDbContext _context; // БД
        private readonly IAssetData _assetdata; // Для парсинга различных данных
        private readonly IMemoryCache _cache; // For cache

        public CurrenciesService(FinanceDbContext context,IAssetData assetdata, IMemoryCache memoryCache)  // Конструктор
        {
            _context = context;
            _assetdata = assetdata;
            _cache = memoryCache;

        }
        public async Task Add(Currency currency)  // Добавление валюты в БД с учетом повторения
        {          
            currency.CharCode = await _assetdata.GetCurrencyCode(currency.NameCurrency);    //получаем код валюты для вычислений

            var oldCurrency = await _context.Currencies.FirstOrDefaultAsync
                (c => c.UserId == currency.UserId && c.CharCode == currency.CharCode);  //Ищем такую же валюту в портфеле, если имеется

            currency.SumCurrencyToRuble = currency.Price * currency.AmountCurrency;

            if (oldCurrency != null)   // если уже есть такая валюта в БД, то цену ставим среднюю
            {
                var totalAmount = oldCurrency.AmountCurrency + currency.AmountCurrency; //Всего валюты
                oldCurrency.Price = Math.Round(((oldCurrency.SumCurrencyToRuble + currency.SumCurrencyToRuble) / totalAmount), 2);   //Средняя цена покупки
                oldCurrency.AmountCurrency = totalAmount;
                
                oldCurrency.SumCurrencyToRuble = Math.Round((oldCurrency.AmountCurrency * oldCurrency.Price), 2); //Обновляем сумму в рублях по сегодняшнему курсу
                oldCurrency.DateAddStock = DateTime.UtcNow;

                _context.Currencies.Update(oldCurrency);
            }
            //Иначе добавляем новую крипту
            else
            {                           
                await _context.Currencies.AddAsync(currency);
            }
            
            await _context.SaveChangesAsync();  // Асинхронно сохраняем изменения в БД
            ClearCurrenciesCache(currency.UserId);  // For clear cache
        }
        public async Task Delete(int id)    //Удаление криптовалюты
        {
            var currency = await _context.Currencies.FindAsync(id);
            if(currency != null)
            {
                _context.Currencies.Remove(currency);
                await _context.SaveChangesAsync();
            }
            ClearCurrenciesCache(currency.UserId);  // For clear cache
        }
        public async Task<Currency?> GetAssetById(int id)  //получение криптовалюты для удаления
        {
            return await _context.Currencies.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<Currency>> GetAssetsByID(int userId)     //Перечисление всей крипты пользователя
        {
            var currency = await _context.Currencies
                .Where(s => s.UserId == userId)
                .ToListAsync(); // Получение таблицы с криптой пользователя           
            return currency;
        }      

        public async Task<IEnumerable<ForChart>> GetChartTicker(int userId) //График по валюте
        {
            var data = await _context.Currencies
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.NameCurrency)
                .Select(g => new ForChart
                {
                    Label = g.Key,
                    Total = g.Sum(e => e.SumCurrencyToRuble)
                })
                .ToListAsync();
            return data;
        }
        public async Task<decimal> GetCurrentCurrenciesSUM(int userId)    // Получение текущего курса Metals
        {
            var cacheKey = $"Currencies:current:{userId}";
            if (_cache.TryGetValue(cacheKey, out decimal cachedSum))
                return cachedSum;

            var currencies = await _context.Currencies
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal totalCurrSum = 0;
            foreach (var currency in currencies)
            {
                var currPrice = await _assetdata.GetCurrencyRate(currency.CharCode);
                totalCurrSum += (currPrice * currency.AmountCurrency);
            }

            _cache.Set(
                cacheKey,
                totalCurrSum,
                TimeSpan.FromHours(1)
            );

            return totalCurrSum;
        }
        public async Task<decimal> GetPurchaseCurrenciesSUM(int userId)    // Получение суммы покупки US Stocks
        {
            var cacheKey = $"Currencies:purchase:{userId}";
            if (_cache.TryGetValue(cacheKey, out decimal cachedSum))
                return cachedSum;

            var currencies = await _context.Currencies
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal totalPurchaseSum = 0;
            foreach (var currency in currencies)
            {
                totalPurchaseSum += currency.SumCurrencyToRuble;
            }
            _cache.Set(
                cacheKey,
                totalPurchaseSum,
                TimeSpan.FromHours(1)
            );
            return totalPurchaseSum;
        }
        private void ClearCurrenciesCache(int userId)
        {
            _cache.Remove($"Currencies:current:{userId}");
            _cache.Remove($"Currencies:purchase:{userId}");
        }























        public async Task FixOldCryptos()   // Для правок в БД
        {
            var cryptos = await _context.Cryptos.ToListAsync();
            decimal rate = await _assetdata.GetCurrencyRate("USD");

            foreach (var crypto in cryptos)
            {
                crypto.SumCrypto = crypto.Price * crypto.AmountCrypto;
                crypto.SumCryptoToRuble = crypto.SumCrypto * rate;
            }

            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Currency>> GetAll()
        {
            var crypto = await _context.Currencies.ToListAsync();  // Перечисление всех данных из БД
            return crypto;
        }
    }
}
