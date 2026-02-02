 using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinancialAssetsApp.Data.Service
{
    public class PlatformStartupsService : IPlatformStartupService
    {
        private readonly FinanceDbContext _context; // БД
        private readonly IMemoryCache _cache; // For cache

        public PlatformStartupsService(FinanceDbContext context, IMemoryCache memoryCache)  // Конструктор
        {
            _context = context;
            _cache = memoryCache;
        }
        public async Task Add(PlatformStartup platformStartup)  // Добавление платформы в БД
        {
            var existPlatform = await _context.PlatformStartups.FirstOrDefaultAsync(plt => plt.UserId == platformStartup.UserId && plt.NamePlatform == platformStartup.NamePlatform); //поиск существующей платформы

            if (existPlatform != null) // если такая платформа существует, то изменяем кол-во компаний и сумму, иначе добавляем в БД с нулевыми значениями
            {
                var startups = await _context.Startups
                    .Where(stup => stup.UserId == existPlatform.UserId && stup.PlatformStartupId == existPlatform.Id)
                    .ToListAsync(); // Находим стартапы на выбранной платформе

                existPlatform.AmountCompanies = startups.Count;
                existPlatform.SumOfStartups = startups
                    .Sum(sum => sum.SumStocks);
                existPlatform.DateAddStock = DateTime.UtcNow;

                _context.PlatformStartups.Update(existPlatform);
            }
            else
            {
                platformStartup.AmountCompanies = 0;    // ставим кол-во 
                platformStartup.SumOfStartups = 0m;     // и сумму по умолчанию 0, так как добавляем новую платформу
                await _context.PlatformStartups.AddAsync(platformStartup);
            }
            await _context.SaveChangesAsync();  // Асинхронно сохраняем изменения в БД
            ClearStartupsCache(platformStartup.UserId);
        }
        public async Task Delete(int id)    //Удаление акции
        {
            var platform = await _context.PlatformStartups.FindAsync(id);
            if(platform != null)
            {
                _context.PlatformStartups.Remove(platform);
                await _context.SaveChangesAsync();
            }
            ClearStartupsCache(platform.UserId);

        }
        public async Task<PlatformStartup?> GetAssetById(int id)  //получение акции для удаления
        {
            return await _context.PlatformStartups.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<PlatformStartup>> GetAssetsByID(int userId)     //Перечисление всех платформ, которые добавлены пользователем
        {
            var platforms = await _context.PlatformStartups
                .Where(s => s.UserId == userId)
                .ToListAsync();
            return platforms;
        }
        
        public async Task<IEnumerable<PlatformStartup>> GetAll()
        { 
            var metal = await _context.PlatformStartups.ToListAsync();  // Перечисление всех данных из БД
            return metal;
        }

        public async Task<IEnumerable<ForChart>> GetChartTicker(int userId) //График по акциям
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
        public async Task<IEnumerable<ForChart>> GetChartCount(int userId) //График по акциям
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
        public async Task<decimal> GetPurchasePlStartupsSUM(int userId)    // Получение суммы покупки Startups
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
