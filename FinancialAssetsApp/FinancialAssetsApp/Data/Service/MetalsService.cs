 using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinancialAssetsApp.Data.Service
{
    public class MetalsService : IMetalsService
    {
        private readonly FinanceDbContext _context; // БД
        private readonly IAssetData _assetdata; // Для парсинга различных курсов

        public MetalsService(FinanceDbContext context,IAssetData assetdata)  // Конструктор
        {
            _context = context;
            _assetdata = assetdata;
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
        }
        public async Task Delete(int id)    //Удаление акции
        {
            var metal = await _context.Metals.FindAsync(id);
            if(metal != null)
            {
                _context.Metals.Remove(metal);
                await _context.SaveChangesAsync();
            }
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
