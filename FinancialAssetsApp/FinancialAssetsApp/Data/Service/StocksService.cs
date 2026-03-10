 using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinancialAssetsApp.Data.Service
{
    public class StocksService : IStocksService
    {
        private readonly FinanceDbContext _context; // Database context
        private readonly IAssetData _assetdata; // For fetching various exchange rates
        private readonly IMemoryCache _cache; // For cache

        public StocksService(FinanceDbContext context, IAssetData assetdata, IMemoryCache memoryCache)  // Constructor
        {
            _context = context;
            _assetdata = assetdata;
            _cache = memoryCache;
        }
        public async Task Add(Stock stock)  // Add stock to DB
        {
            var temp = stock.Ticker.ToUpper();  // Normalize ticker to uppercase
            stock.Ticker = temp;
            stock.SumStocks = Math.Round(stock.Price * stock.AmountStock, 2);

            var existStock = await _context.Stocks.FirstOrDefaultAsync(stck => stck.UserId == stock.UserId && stck.Ticker == stock.Ticker); // Search for existing stock entry

            stock.SumStocks = stock.Price * stock.AmountStock;

            if (existStock != null) // If stock already exists, calculate average price; otherwise add new entry
            {
                var totalAmount = existStock.AmountStock + stock.AmountStock;
                existStock.Price = Math.Round((existStock.SumStocks + stock.SumStocks) / totalAmount, 2);
                existStock.AmountStock = totalAmount;
                existStock.SumStocks = existStock.Price * existStock.AmountStock;
                existStock.DateAddStock = DateTime.UtcNow;

                _context.Stocks.Update(existStock);
            }
            else
            {
                await _context.Stocks.AddAsync(stock);
            }
            await _context.SaveChangesAsync();  // Save changes to DB asynchronously
            ClearStocksCache(stock.UserId);   // Update cache
        }
        public async Task Delete(int id)    // Delete stock asset by ID
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock != null)
            {
                _context.Stocks.Remove(stock);
                await _context.SaveChangesAsync();
                ClearStocksCache(stock.UserId);   // Update cache
            }
        }
        public async Task<Stock?> GetAssetById(int id)  // Get stock by ID (used for deletion)
        {
            return await _context.Stocks.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<Stock>> GetAssetsByID(int userId)     // Get all stocks for a user
        {
            var stocks = await _context.Stocks
                .Where(s => s.UserId == userId)
                .ToListAsync();
            return stocks;
        }

        public async Task<IEnumerable<Stock>> GetAll()
        {
            var stocks = await _context.Stocks.ToListAsync();  // Fetch all records from DB
            return stocks;
        }

        public async Task<IEnumerable<ForChart>> GetChartTicker(int userId) // Chart data grouped by stock ticker
        {
            var data = await _context.Stocks
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.Ticker)
                .Select(g => new ForChart
                {
                    Label = g.Key,
                    Total = g.Sum(e => e.SumStocks)
                })
                .ToListAsync();
            return data;
        }
        public async Task<decimal> GetPurchaseStocksSUM(int userId)    // Get total purchase sum for RUB stocks
        {
            var cacheKey = $"Stocks:purchase:{userId}";
            if (_cache.TryGetValue(cacheKey, out decimal cachedSum))
                return cachedSum;

            var stocks = await _context.Stocks
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal totalPurchaseSum = 0;
            foreach (var stock in stocks)
            {
                totalPurchaseSum += stock.SumStocks;
            }
            _cache.Set(
                cacheKey,
                totalPurchaseSum,
                TimeSpan.FromHours(1)
            );
            return totalPurchaseSum;
        }
        private void ClearStocksCache(int userId)
        {
            //_cache.Remove($"Stocks:current:{userId}");
            _cache.Remove($"Stocks:purchase:{userId}");
        }




















        /*public async Task<IEnumerable<ForChart>> GetChartCountry(int userId)    //График по странам
        {
            var data = await _context.Stocks
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.Country)
                .Select(g => new ForChart
                {
                    Label = g.Key ?? "не указано",
                    Total = g.Sum(e => e.SumStocksToRuble).GetValueOrDefault()
                })
                .ToListAsync();
            return data;
        }












        /*public async Task FixOldStocks()
        {
            var stocks = await _context.Stocks
                .Where(s => s.SumStocksToRuble == null && s.AmountStock > 0)
                .ToListAsync();

            foreach (var stock in stocks)
            {
                decimal rate = 1;   // Если акции российские, то сумма остается той же
                if (stock.Country == "США")
                    rate = await _assetData.GetCurrencyRate("USD");
                stock.SumStocks = stock.Price * stock.AmountStock;
                stock.SumStocksToRuble = stock.SumStocks * rate;
            }

            await _context.SaveChangesAsync();
        }*/
    }
}
