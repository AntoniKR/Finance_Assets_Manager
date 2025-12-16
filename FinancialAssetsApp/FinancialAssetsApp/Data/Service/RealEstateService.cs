 using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinancialAssetsApp.Data.Service
{
    public class RealEstateService : IRealEstateService
    {
        private readonly FinanceDbContext _context; // БД
        private readonly IAssetData _assetdata; // Для парсинга различных курсов

        public RealEstateService(FinanceDbContext context,IAssetData assetdata)  // Конструктор
        {
            _context = context;
            _assetdata = assetdata;
        }
        public async Task Add(RealEstate realestate)  // Добавление недвижимости в БД
        {
            realestate.SumEstate = realestate.Price * realestate.AmountEstate;
            await _context.RealEstates.AddAsync(realestate);         
            await _context.SaveChangesAsync();  // Асинхронно сохраняем изменения в БД
        }
        public async Task Delete(int id)    //Удаление недвижимости
        {
            var realestate = await _context.RealEstates.FindAsync(id);
            if(realestate != null)
            {
                _context.RealEstates.Remove(realestate);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<RealEstate?> GetAssetById(int id)  //получение недвижимости для удаления
        {
            return await _context.RealEstates.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<RealEstate>> GetAssetsByID(int userId)     //Перечисление всей недвижимости пользователя
        {
            var realestate = await _context.RealEstates
                .Where(s => s.UserId == userId)
                .ToListAsync();
            return realestate;
        }
        
        public async Task<IEnumerable<RealEstate>> GetAll()
        { 
            var realestate = await _context.RealEstates.ToListAsync();  // Перечисление всех данных из БД
            return realestate;
        }

        public async Task<IEnumerable<ForChart>> GetChartCities(int userId) //График по городам недвижимости
        {
            var data = await _context.RealEstates
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.CityEstate)
                .Select(g => new ForChart
                {
                    Label = g.Key,
                    Total = g.Sum(e => e.SumEstate)
                })
                .ToListAsync();
            return data;
        }
        public async Task<IEnumerable<ForChart>> GetChartType(int userId) //График по типу недвижимости
        {
            var data = await _context.RealEstates
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.TypeEstate)
                .Select(g => new ForChart
                {
                    Label = g.Key,
                    Total = g.Sum(e => e.SumEstate)
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
