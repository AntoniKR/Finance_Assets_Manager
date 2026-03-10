 using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinancialAssetsApp.Data.Service
{
    public class StartupsService : IStartupService
    {
        private readonly FinanceDbContext _context; // Database context
        private readonly IAssetData _assetdata; // For fetching various exchange rates

        public StartupsService(FinanceDbContext context, IAssetData assetdata)  // Constructor
        {
            _context = context;
            _assetdata = assetdata;
        }
        public async Task Add(Startup startup)  // Add startup to DB
        {
            var existstartup = await _context.Startups.FirstOrDefaultAsync(stup => stup.UserId == startup.UserId && stup.PlatformStartupId == startup.PlatformStartupId && stup.NameCompany == startup.NameCompany); // Search for existing startup

            if (existstartup != null) // If startup already exists in DB, update share count and total sum; otherwise add new entry
            {
                var totalAmount = existstartup.AmountStock + startup.AmountStock;
                existstartup.Price = Math.Round((existstartup.SumStocks + startup.SumStocks) / totalAmount, 2);
                existstartup.AmountStock = totalAmount;
                existstartup.SumStocks = existstartup.Price * existstartup.AmountStock;
                existstartup.DateAddStock = DateTime.UtcNow;

                _context.Startups.Update(existstartup);
            }
            else
            {
                startup.SumStocks = startup.Price * startup.AmountStock;
                await _context.Startups.AddAsync(startup);
            }
            await _context.SaveChangesAsync();  // Save changes to DB asynchronously

            var platform = await _context.PlatformStartups.FirstOrDefaultAsync(plt => plt.UserId == startup.UserId && plt.Id == startup.PlatformStartupId); // Find the platform added by the user
            var startups = await _context.Startups
                .Where(st => st.UserId == startup.UserId && st.PlatformStartupId == startup.PlatformStartupId)
                .ToListAsync();     // Get all user's startups on this platform

            platform.SumOfStartups = startups.Sum(s => s.SumStocks);    // Update total sum of startups on the platform

            if (existstartup == null)
            {
                platform.AmountCompanies = startups.Count();    // Update total count of startups on the platform
            }
            _context.PlatformStartups.Update(platform);
            await _context.SaveChangesAsync();  // Save changes to DB asynchronously
        }
        public async Task Delete(int id)    // Delete startup by ID
        {
            var startup = await _context.Startups.FindAsync(id);
            if (startup != null)
            {
                var platform = await _context.PlatformStartups.FirstOrDefaultAsync(plt => plt.UserId == startup.UserId && plt.Id == startup.PlatformStartupId); // Find the platform added by the user

                platform.SumOfStartups -= startup.SumStocks;
                _context.PlatformStartups.Update(platform);
                platform.AmountCompanies -= 1;
                platform.DateAddStock = DateTime.UtcNow;
                _context.Startups.Remove(startup);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Startup?> GetAssetById(int id)  // Get startup by ID (used for deletion)
        {
            return await _context.Startups.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<Startup>> GetAssetsByID(int userId)     // Get all startups added by the user
        {
            var startups = await _context.Startups
                .Where(s => s.UserId == userId)
                .ToListAsync();
            return startups;
        }
        public async Task<int> GetPlatformId(string namePlatform) // Get platform ID by name
        {
            var platform = await _context.PlatformStartups.FirstOrDefaultAsync(name => name.NamePlatform == namePlatform);
            return platform.Id;
        }
        public async Task<IEnumerable<PlatformStartup>> GetAllPlatforms(int userId) // Get list of platforms when creating a startup
        {
            var platforms = await _context.PlatformStartups
                .Where(s => s.UserId == userId)
                .ToListAsync();
            return platforms;
        }
        public async Task<IEnumerable<Startup>> GetAll()
        {
            var startup = await _context.Startups.ToListAsync();  // Fetch all records from DB
            return startup;
        }
        public async Task<IEnumerable<ForChart>> GetChartTicker(int userId) // Chart data grouped by company name
        {
            var data = await _context.Startups
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.NameCompany)
                .Select(g => new ForChart
                {
                    Label = g.Key,
                    Total = g.Sum(e => e.SumStocks)
                })
                .ToListAsync();
            return data;
        }
        public async Task<IEnumerable<ForChart>> GetChartCount(int userId) // Chart data grouped by share count per company
        {
            var data = await _context.Startups
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.NameCompany)
                .Select(g => new ForChart
                {
                    Label = g.Key,
                    Total = g.Sum(e => e.AmountStock)
                })
                .ToListAsync();
            return data;
        }

        /*public async Task<decimal> GetPurchaseStartupsSUM(int userId)    // Get total purchase sum for startups
            {
                var cacheKey = $"Metals:purchase:{userId}";
                if (_cache.TryGetValue(cacheKey, out decimal cachedSum))
                    return cachedSum;

                var metals = await _context.Metals
                    .Where(s => s.UserId == userId)
                    .ToListAsync();

                decimal totalPurchaseSum = 0;
                foreach (var metal in metals)
                {
                    totalPurchaseSum += metal.SumMetals;
                }
                _cache.Set(
                    cacheKey,
                    totalPurchaseSum,
                    TimeSpan.FromHours(1)
                );
                return totalPurchaseSum;
            }*/























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
