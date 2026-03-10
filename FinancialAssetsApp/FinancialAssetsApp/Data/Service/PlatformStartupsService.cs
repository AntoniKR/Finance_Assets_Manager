 using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinancialAssetsApp.Data.Service
{
    public class PlatformStartupsService : IPlatformStartupService
    {
        private readonly FinanceDbContext _context; // Database context
        private readonly IMemoryCache _cache; // For cache

        public PlatformStartupsService(FinanceDbContext context, IMemoryCache memoryCache)  // Constructor
        {
            _context = context;
            _cache = memoryCache;
        }
        public async Task Add(PlatformStartup platformStartup)  // Add platform to DB
        {
            var existPlatform = await _context.PlatformStartups.FirstOrDefaultAsync(plt => plt.UserId == platformStartup.UserId && plt.NamePlatform == platformStartup.NamePlatform); // Search for existing platform

            if (existPlatform != null) // If platform already exists, update company count and total sum; otherwise add with default zero values
            {
                var startups = await _context.Startups
                    .Where(stup => stup.UserId == existPlatform.UserId && stup.PlatformStartupId == existPlatform.Id)
                    .ToListAsync(); // Get all startups on the selected platform

                existPlatform.AmountCompanies = startups.Count;
                existPlatform.SumOfStartups = startups
                    .Sum(sum => sum.SumStocks);
                existPlatform.DateAddStock = DateTime.UtcNow;

                _context.PlatformStartups.Update(existPlatform);
            }
            else
            {
                platformStartup.AmountCompanies = 0;    // Set default company count to 0 for new platform
                platformStartup.SumOfStartups = 0m;     // Set default sum to 0 for new platform
                await _context.PlatformStartups.AddAsync(platformStartup);
            }
            await _context.SaveChangesAsync();  // Save changes to DB asynchronously
            ClearStartupsCache(platformStartup.UserId);
        }
        public async Task Delete(int id)    // Delete platform by ID
        {
            var platform = await _context.PlatformStartups.FindAsync(id);
            if (platform != null)
            {
                _context.PlatformStartups.Remove(platform);
                await _context.SaveChangesAsync();
            }
            ClearStartupsCache(platform.UserId);
        }
        public async Task<PlatformStartup?> GetAssetById(int id)  // Get platform by ID (used for deletion)
        {
            return await _context.PlatformStartups.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<PlatformStartup>> GetAssetsByID(int userId)     // Get all platforms added by the user
        {
            var platforms = await _context.PlatformStartups
                .Where(s => s.UserId == userId)
                .ToListAsync();
            return platforms;
        }

        public async Task<IEnumerable<PlatformStartup>> GetAll()
        {
            var metal = await _context.PlatformStartups.ToListAsync();  // Fetch all records from DB
            return metal;
        }

        public async Task<IEnumerable<ForChart>> GetChartTicker(int userId) // Chart data grouped by platform name (by sum)
        {
            var data = await _context.PlatformStartups
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.NamePlatform)
                .Select(g => new ForChart
                {
                    Label = g.Key,
                    Total = g.Sum(e => e.SumOfStartups) ?? 0
                })
                .ToListAsync();
            return data;
        }
        public async Task<IEnumerable<ForChart>> GetChartCount(int userId) // Chart data grouped by platform name (by company count)
        {
            var data = await _context.PlatformStartups
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.NamePlatform)
                .Select(g => new ForChart
                {
                    Label = g.Key,
                    Total = g.Sum(e => e.AmountCompanies) ?? 0
                })
                .ToListAsync();
            return data;
        }
        public async Task<decimal> GetPurchasePlStartupsSUM(int userId)    // Get total purchase sum for startups
        {
            var cacheKey = $"Startups:purchase:{userId}";
            if (_cache.TryGetValue(cacheKey, out decimal cachedSum))
                return cachedSum;

            var startups = await _context.PlatformStartups
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal? totalPurchaseSum = 0;
            foreach (var startup in startups)
            {
                totalPurchaseSum += startup.SumOfStartups;
            }
            _cache.Set(
                cacheKey,
                totalPurchaseSum,
                TimeSpan.FromHours(1)
            );
            return totalPurchaseSum ?? 0;
        }
        private void ClearStartupsCache(int userId)
        {
            _cache.Remove($"Metals:purchase:{userId}");
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
