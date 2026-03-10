using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace FinancialAssetsApp.Data.Service
{
    public class StocksUSDservice : IStocksUSDService
    {
        private readonly FinanceDbContext _context; // Database context
        private readonly IAssetData _assetdata; // For fetching various exchange rates
        private readonly IMemoryCache _cache; // For cache

        public StocksUSDservice(FinanceDbContext context, IAssetData assetdata, IMemoryCache memoryCache)  // Constructor
        {
            _context = context;
            _assetdata = assetdata;
            _cache = memoryCache;
        }
        public async Task Add(StockUSD stock)  // Add USD stock to DB
        {
            decimal rate = await _assetdata.GetCurrencyRate("USD");   // Get USD rate for converting to rubles
            var temp = stock.Ticker.ToUpper();  // Normalize ticker to uppercase
            stock.Ticker = temp;
            stock.SumStocks = Math.Round((stock.Price * stock.AmountStock), 2);
            stock.SumStocksToRuble = Math.Round((stock.SumStocks * rate), 2);

            var existStock = await _context.StocksUSD.FirstOrDefaultAsync(stck => stck.UserId == stock.UserId && stck.Ticker == stock.Ticker); // Search for existing stock entry

            if (existStock != null) // If stock already exists, calculate average price; otherwise add new entry
            {
                var totalAmount = existStock.AmountStock + stock.AmountStock;
                existStock.Price = Math.Round(((existStock.SumStocks + stock.SumStocks) / totalAmount), 2);
                existStock.AmountStock = totalAmount;
                existStock.SumStocks = Math.Round((existStock.Price * existStock.AmountStock), 2);
                existStock.SumStocksToRuble = Math.Round((existStock.SumStocks * rate), 2);
                existStock.DateAddStock = DateTime.UtcNow;

                _context.StocksUSD.Update(existStock);  // Update existing row in DB
            }
            else
            {   // Otherwise add as a new stock entry
                await _context.StocksUSD.AddAsync(stock);
            }
            await _context.SaveChangesAsync();  // Save changes to DB asynchronously
            ClearUSStocksCache(stock.UserId);   // Update cache
        }
        public async Task Delete(int id)    // Delete USD stock by ID
        {
            var stock = await _context.StocksUSD.FindAsync(id);
            if (stock != null)
            {
                _context.StocksUSD.Remove(stock);
                await _context.SaveChangesAsync();
                ClearUSStocksCache(stock.UserId);   // Update cache
            }
        }
        public async Task<StockUSD?> GetAssetById(int id)  // Get USD stock by ID (used for deletion)
        {
            return await _context.StocksUSD.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<StockUSD>> GetAssetsByID(int userId)     // Get all USD stocks for a user
        {
            var stocks = await _context.StocksUSD
                .Where(s => s.UserId == userId)
                .ToListAsync();
            return stocks;
        }

        public async Task<IEnumerable<StockUSD>> GetAll()
        {
            var stocks = await _context.StocksUSD.ToListAsync();  // Fetch all records from DB
            return stocks;
        }

        public async Task<IEnumerable<ForChart>> GetChartTicker(int userId) // Chart data grouped by USD stock ticker
        {
            var data = await _context.StocksUSD
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.Ticker)
                .Select(g => new ForChart
                {
                    Label = g.Key,
                    Total = g.Sum(e => e.SumStocksToRuble)
                })
                .ToListAsync();
            return data;
        }
        public async Task<decimal> GetCurrentUSStocksSUM(int userId)    // Get current total value of USD stocks
        {
            var cacheKey = $"StocksUSD:current:{userId}";
            if (_cache.TryGetValue(cacheKey, out decimal cachedSum))
                return cachedSum;

            var usStocks = await _context.StocksUSD
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal totalCurrSum = 0;
            foreach (var stock in usStocks)
            {
                totalCurrSum += stock.SumStocks;
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
        public async Task<decimal> GetPurchaseUSStocksSUM(int userId)    // Get total purchase sum for USD stocks
        {
            var cacheKey = $"StocksUSD:purchase:{userId}";
            if (_cache.TryGetValue(cacheKey, out decimal cachedSum))
                return cachedSum;

            var usStocks = await _context.StocksUSD
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal totalPurchaseSum = 0;
            foreach (var stock in usStocks)
            {
                totalPurchaseSum += stock.SumStocksToRuble;
            }
            _cache.Set(
                cacheKey,
                totalPurchaseSum,
                TimeSpan.FromHours(1)
            );
            return totalPurchaseSum;
        }
        private void ClearUSStocksCache(int userId)
        {
            _cache.Remove($"StocksUSD:current:{userId}");
            _cache.Remove($"StocksUSD:purchase:{userId}");
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
