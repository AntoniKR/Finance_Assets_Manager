 using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinancialAssetsApp.Data.Service
{
    public class MetalsService : IMetalsService
    {
        private readonly FinanceDbContext _context; // БД
        private readonly IAssetData _assetdata; // For parse different data
        private readonly IMemoryCache _cache; // For cache


        public MetalsService(FinanceDbContext context,IAssetData assetdata, IMemoryCache memoryCache)  // Конструктор
        {
            _context = context;
            _assetdata = assetdata;
            _cache = memoryCache;
        }
        public async Task Add(Metal metal)  // Добавление металла в БД
        {
            var existMetal = await _context.Metals.FirstOrDefaultAsync(mtl => mtl.UserId == metal.UserId && mtl.NameMetal == metal.NameMetal); //поиск существующего

            metal.SumMetals = metal.Price * metal.AmountMetal;

            if (existMetal != null) // если такой металл есть, то усредняем, иначе добавляем новый
            {
                var totalAmount = existMetal.AmountMetal + metal.AmountMetal;   
                existMetal.Price = Math.Round((existMetal.SumMetals + metal.SumMetals) / totalAmount, 2);
                existMetal.AmountMetal = totalAmount;
                existMetal.SumMetals = existMetal.Price * existMetal.AmountMetal;
                existMetal.DateAddStock = DateTime.UtcNow;

                _context.Metals.Update(existMetal);
            }
            else
            {
                await _context.Metals.AddAsync(metal);
            }
            await _context.SaveChangesAsync();  // Асинхронно сохраняем изменения в БД
            ClearMetalsCache(metal.UserId);   // Update cache
        }
        public async Task Delete(int id)    //Удаление акции
        {
            var metal = await _context.Metals.FindAsync(id);
            if(metal != null)
            {
                _context.Metals.Remove(metal);
                await _context.SaveChangesAsync();
            }
            ClearMetalsCache(metal.UserId);   // Update cache
        }
        public async Task<Metal?> GetAssetById(int id)  //получение акции для удаления
        {
            return await _context.Metals.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<Metal>> GetAssetsByID(int userId)     //Перечисление всех акций пользователя
        {
            var metal = await _context.Metals
                .Where(s => s.UserId == userId)
                .ToListAsync();
            return metal;
        }
        
        public async Task<IEnumerable<Metal>> GetAll()
        { 
            var metal = await _context.Metals.ToListAsync();  // Перечисление всех данных из БД
            return metal;
        }

        public async Task<IEnumerable<ForChart>> GetChartTicker(int userId) //График по акциям
        {
            var data = await _context.Metals
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.NameMetal)
                .Select(g => new ForChart
                {
                    Label = g.Key,
                    Total = g.Sum(e => e.SumMetals)
                })
                .ToListAsync();
            return data;
        }

        public async Task<decimal> GetCurrentMetalsSUM(int userId)    // Получение текущего курса Metals
        {
            var cacheKey = $"Metals:current:{userId}";
            if (_cache.TryGetValue(cacheKey, out decimal cachedSum))
                return cachedSum;

            var metals = await _context.Metals
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal totalCurrSum = 0;
            foreach (var metal in metals)
            {
                var currPrice = await _assetdata.GetMetalPrice(metal.NameMetal);
                totalCurrSum += (currPrice * metal.AmountMetal);
            }

            _cache.Set(
                cacheKey,
                totalCurrSum,
                TimeSpan.FromHours(1)
            );

            return totalCurrSum;
        }
        public async Task<decimal> GetPurchaseMetalsSUM(int userId)    // Получение суммы покупки US Stocks
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
        }
        private void ClearMetalsCache(int userId)
        {
            _cache.Remove($"Metals:current:{userId}");
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
