 using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinancialAssetsApp.Data.Service
{
    public class TransportService : ITransportService
    {
        private readonly FinanceDbContext _context; // БД
        public TransportService(FinanceDbContext context)  // Конструктор
        {
            _context = context;
        }
        public async Task Add(Transport transport)  // Добавление транспорта в БД
        {
            await _context.Transports.AddAsync(transport);         
            await _context.SaveChangesAsync();  // Асинхронно сохраняем изменения в БД
        }
        public async Task Delete(int id)    //Удаление транспорта
        {
            var transport = await _context.Transports.FindAsync(id);
            if(transport != null)
            {
                _context.Transports.Remove(transport);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Transport?> GetAssetById(int id)  //получение транспорта для удаления
        {
            return await _context.Transports.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<Transport>> GetAssetsByID(int userId)     //Перечисление всего транспорта пользователя
        {
            var transport = await _context.Transports
                .Where(s => s.UserId == userId)
                .ToListAsync();
            return transport;
        }
        
        public async Task<IEnumerable<Transport>> GetAll()
        { 
            var transport = await _context.Transports.ToListAsync();  // Перечисление всех данных из БД
            return transport;
        }

        public async Task<IEnumerable<ForChart>> GetChartTypeTrans(int userId) //График по типу транспорта
        {
            var data = await _context.Transports
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.TypeTransport)
                .Select(g => new ForChart
                {
                    Label = g.Key,
                    Total = g.Sum(e => e.Price)
                })
                .ToListAsync();
            return data;
        }
        public async Task<IEnumerable<ForChart>> GetChartSumTrans(int userId) //График по сумме транспорта
        {
            var data = await _context.Transports
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.NameTransport)
                .Select(g => new ForChart
                {
                    Label = g.Key,
                    Total = g.Sum(e => e.Price)
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
