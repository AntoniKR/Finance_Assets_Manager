 using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinancialAssetsApp.Data.Service
{
    public class StartupsService : IStartupService
    {
        private readonly FinanceDbContext _context; // БД
        private readonly IAssetData _assetdata; // Для парсинга различных курсов

        public StartupsService(FinanceDbContext context,IAssetData assetdata)  // Конструктор
        {
            _context = context;
            _assetdata = assetdata;
        }
        public async Task Add(Startup startup)  // Добавление стартапа в БД
        {
            var existstartup = await _context.Startups.FirstOrDefaultAsync(stup => stup.UserId == startup.UserId && stup.PlatformStartupId == startup.PlatformStartupId && stup.NameCompany == startup.NameCompany); //поиск существующего стартапа

            if (existstartup != null) // если такой стартап существует в БД, то изменяем кол-во акций стартапа и сумму, иначе добавляем в БД
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
            await _context.SaveChangesAsync();  // Асинхронно сохраняем изменения в БД

            var platform = await _context.PlatformStartups.FirstOrDefaultAsync(plt => plt.UserId == startup.UserId && plt.Id == startup.PlatformStartupId); //Находим платформу, добавленную пользователем
            var startups = await _context.Startups
                .Where(st => st.UserId == startup.UserId && st.PlatformStartupId == startup.PlatformStartupId)
                .ToListAsync();     // Находим все стартапы у пользователя на платформе

            platform.SumOfStartups = startups.Sum(s => s.SumStocks);    //Обновляем сумму стартапов на платформе

            if (existstartup == null)
            {
                platform.AmountCompanies = startups.Count();    //Обновляем кол-во стартапов на платформе
            }
            _context.PlatformStartups.Update(platform);           
            await _context.SaveChangesAsync();  // Асинхронно сохраняем изменения в БД
        }
        public async Task Delete(int id)    //Удаление стартапа
        {
            var startup = await _context.Startups.FindAsync(id);
            if(startup != null)
            {
                var platform = await _context.PlatformStartups.FirstOrDefaultAsync(plt => plt.UserId == startup.UserId && plt.Id == startup.PlatformStartupId); //Находим платформу, добавленную пользователем

                platform.SumOfStartups -= startup.SumStocks;
                _context.PlatformStartups.Update(platform);
                platform.AmountCompanies -= 1;
                platform.DateAddStock = DateTime.UtcNow;
                _context.Startups.Remove(startup);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Startup?> GetAssetById(int id)  //получение компании для удаления
        {
            return await _context.Startups.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<Startup>> GetAssetsByID(int userId)     //Перечисление всех стартапов, которые добавлены пользователем
        {
            var startups = await _context.Startups
                .Where(s => s.UserId == userId)
                .ToListAsync();
            return startups;
        }
        public async Task<int> GetPlatformId(string namePlatform) // Получение Id платформы
        {
            var platform = await _context.PlatformStartups.FirstOrDefaultAsync(name => name.NamePlatform == namePlatform);
            return platform.Id;
        }
        public async Task<IEnumerable<PlatformStartup>> GetAllPlatforms(int userId) // Получение списка платформ при создании стартапа
        {
            var platforms = await _context.PlatformStartups
                .Where(s => s.UserId == userId)
                .ToListAsync();
            return platforms;
        }


        public async Task<IEnumerable<Startup>> GetAll()
        { 
            var startup = await _context.Startups.ToListAsync();  // Перечисление всех данных из БД
            return startup;
        }

        public async Task<IEnumerable<ForChart>> GetChartTicker(int userId) //График по акциям
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
        public async Task<IEnumerable<ForChart>> GetChartCount(int userId) //График по акциям
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
