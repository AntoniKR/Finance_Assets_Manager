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
        private readonly FinanceDbContext _context; // Database context
        private readonly IAssetData _assetdata; // For fetching various asset data
        private readonly IMemoryCache _cache; // For cache

        public CurrenciesService(FinanceDbContext context, IAssetData assetdata, IMemoryCache memoryCache)  // Constructor
        {
            _context = context;
            _assetdata = assetdata;
            _cache = memoryCache;
        }
        public async Task Add(Currency currency)  // Add currency to DB, merging with existing entry if present
        {
            currency.CharCode = await _assetdata.GetCurrencyCode(currency.NameCurrency);    // Get currency code for calculations

            var oldCurrency = await _context.Currencies.FirstOrDefaultAsync
                (c => c.UserId == currency.UserId && c.CharCode == currency.CharCode);  // Search for the same currency in the portfolio

            currency.SumCurrencyToRuble = currency.Price * currency.AmountCurrency;

            if (oldCurrency != null)   // If currency already exists in DB, calculate average purchase price
            {
                var totalAmount = oldCurrency.AmountCurrency + currency.AmountCurrency; // Total currency amount
                oldCurrency.Price = Math.Round(((oldCurrency.SumCurrencyToRuble + currency.SumCurrencyToRuble) / totalAmount), 2);   // Average purchase price
                oldCurrency.AmountCurrency = totalAmount;

                oldCurrency.SumCurrencyToRuble = Math.Round((oldCurrency.AmountCurrency * oldCurrency.Price), 2); // Update ruble sum at today's rate
                oldCurrency.DateAddStock = DateTime.UtcNow;

                _context.Currencies.Update(oldCurrency);
            }
            // Otherwise add as a new currency entry
            else
            {
                await _context.Currencies.AddAsync(currency);
            }

            await _context.SaveChangesAsync();  // Save changes to DB asynchronously
            ClearCurrenciesCache(currency.UserId);  // Clear cache
        }
        public async Task Delete(int id)    // Delete currency asset by ID
        {
            var currency = await _context.Currencies.FindAsync(id);
            if (currency != null)
            {
                _context.Currencies.Remove(currency);
                await _context.SaveChangesAsync();
            }
            ClearCurrenciesCache(currency.UserId);  // Clear cache
        }
        public async Task<Currency?> GetAssetById(int id)  // Get currency asset by ID (used for deletion)
        {
            return await _context.Currencies.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<Currency>> GetAssetsByID(int userId)     // Get all currency assets for a user
        {
            var currency = await _context.Currencies
                .Where(s => s.UserId == userId)
                .ToListAsync(); // Fetch user's currency table from DB
            return currency;
        }

        public async Task<IEnumerable<ForChart>> GetChartTicker(int userId) // Chart data grouped by currency name
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
        public async Task<decimal> GetCurrentCurrenciesSUM(int userId)    // Get current total value of all currencies
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
        public async Task<decimal> GetPurchaseCurrenciesSUM(int userId)    // Get total purchase sum for currencies
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
