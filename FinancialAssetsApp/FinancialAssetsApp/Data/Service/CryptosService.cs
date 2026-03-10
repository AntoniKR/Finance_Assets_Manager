using BCrypt.Net;
using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinancialAssetsApp.Data.Service
{
    public class CryptosService : ICryptosService
    {
        private readonly FinanceDbContext _context; // Database context
        private readonly IAssetData _assetdata; // For fetching external asset data
        private readonly IMemoryCache _cache; // For cache
        public CryptosService(FinanceDbContext context, IAssetData assetdata, IMemoryCache memoryCache)  // Constructor
        {
            _context = context;
            _assetdata = assetdata;
            _cache = memoryCache;
        }
        public async Task Add(Crypto crypto)  // Add crypto to DB, merging with existing ticker if present
        {
            decimal rate = await _assetdata.GetCurrencyRate("USD");   // Get current USD exchange rate
            var temp = crypto.Ticker.ToUpper();  // Normalize ticker to uppercase
            var oldTicker = await _context.Cryptos.FirstOrDefaultAsync
                (c => c.UserId == crypto.UserId && c.Ticker == crypto.Ticker);

            crypto.SumCrypto = Math.Round(crypto.Price * crypto.AmountCrypto, 4);
            crypto.SumCryptoToRuble = Math.Round(crypto.SumCrypto * rate, 2);

            if (oldTicker != null)   // If ticker already exists in DB, calculate average purchase price
            {
                var totalAmount = Math.Round((oldTicker.AmountCrypto + crypto.AmountCrypto), 4); // Total crypto amount
                oldTicker.Price = Math.Round(((oldTicker.SumCryptoToRuble + crypto.SumCryptoToRuble) / totalAmount), 4);    // Average purchase price
                oldTicker.AmountCrypto = totalAmount;
                oldTicker.SumCrypto = oldTicker.Price * oldTicker.AmountCrypto;
                oldTicker.SumCryptoToRuble = oldTicker.SumCrypto * rate;
                oldTicker.DateAddStock = DateTime.UtcNow;

                _context.Cryptos.Update(oldTicker);
            }
            // Otherwise add as a new crypto entry
            else
            {
                await _context.Cryptos.AddAsync(crypto);
            }

            await _context.SaveChangesAsync();  // Save changes to DB asynchronously
            ClearCryptoCache(crypto.UserId);
        }
        public async Task Delete(int id)    // Delete crypto asset by ID
        {
            var crypto = await _context.Cryptos.FindAsync(id);
            if (crypto != null)
            {
                _context.Cryptos.Remove(crypto);
                await _context.SaveChangesAsync();
                ClearCryptoCache(crypto.UserId);     // Update cache
            }
        }
        public async Task<Crypto?> GetAssetById(int id)  // Get crypto asset by ID (used for deletion)
        {
            return await _context.Cryptos.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<Crypto>> GetAssetsByID(int userId)     // Get all crypto assets for a user
        {
            var crypto = await _context.Cryptos
                .Where(s => s.UserId == userId)
                .ToListAsync(); // Fetch user's crypto table from DB
            return crypto;
        }

        public async Task<IEnumerable<ForChart>> GetChartTicker(int userId) // Get chart data grouped by crypto ticker
        {
            var data = await _context.Cryptos
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.Ticker)
                .Select(g => new ForChart
                {
                    Label = g.Key,
                    Total = g.Sum(e => e.SumCryptoToRuble)
                })
                .ToListAsync();
            return data;
        }
        public async Task<decimal> GetCurrentCryptoSUM(int userId)    // Get current total crypto value in rubles
        {
            var cacheKey = $"Crypto:current:{userId}";
            if (_cache.TryGetValue(cacheKey, out decimal cachedSum))
                return cachedSum;

            var cryptos = await _context.Cryptos
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal totalCurrSum = 0;
            foreach (var crypto in cryptos)
            {
                totalCurrSum += crypto.SumCrypto;
            }
            var usdRate = await _assetdata.GetCurrencyRate("USD");
            totalCurrSum *= usdRate;

            _cache.Set(
                cacheKey,
                totalCurrSum,
                TimeSpan.FromHours(1)
            );

            return totalCurrSum;
        }
        public async Task<decimal> GetPurchaseCryptoSUM(int userId)    // Get total purchase sum for crypto
        {
            var cacheKey = $"Crypto:purchase:{userId}";
            if (_cache.TryGetValue(cacheKey, out decimal cachedSum))
                return cachedSum;

            var cryptos = await _context.Cryptos
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal totalPurchaseSum = 0;
            foreach (var crypto in cryptos)
            {
                totalPurchaseSum += crypto.SumCryptoToRuble;
            }
            _cache.Set(
                cacheKey,
                totalPurchaseSum,
                TimeSpan.FromHours(1)
            );
            return totalPurchaseSum;
        }
        private void ClearCryptoCache(int userId)
        {
            _cache.Remove($"Crypto:current:{userId}");
            _cache.Remove($"Crypto:purchase:{userId}");
        }















        public async Task FixOldCryptos()   // Utility method for manual DB corrections
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
        public async Task<IEnumerable<Crypto>> GetAll()
        {
            var crypto = await _context.Cryptos.ToListAsync();  // Fetch all records from DB
            return crypto;
        }
    }
}
