using BCrypt.Net;
using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinancialAssetsApp.Data.Service
{
    public class CryptosService : ICryptosService
    {
        private readonly FinanceDbContext _context; // БД
        private readonly IAssetData _assetdata; // Для парсинга различных данных

        public CryptosService(FinanceDbContext context,IAssetData assetdata)  // Конструктор
        {
            _context = context;
            _assetdata = assetdata;
        }
        public async Task Add(Crypto crypto)  // Добавление крипты в БД с учетом повторения
        {
            decimal rate = await _assetdata.GetCurrencyRate("USD"); ;   // Курс доллара
            var temp = crypto.Ticker.ToUpper();  //Перевод в верхний регистр
            var oldTicker = await _context.Cryptos.FirstOrDefaultAsync
                (c => c.UserId == crypto.UserId && c.Ticker == crypto.Ticker);

            crypto.SumCrypto = Math.Round(crypto.Price * crypto.AmountCrypto, 4);
            crypto.SumCryptoToRuble = Math.Round(crypto.SumCrypto * rate, 2);



            if (oldTicker != null)   // если уже есть такой тикер в БД, то цену ставим среднюю
            {
                var totalAmount = Math.Round((oldTicker.AmountCrypto + crypto.AmountCrypto), 4); //Всего крипты
                oldTicker.Price = Math.Round(((oldTicker.SumCryptoToRuble + crypto.SumCryptoToRuble) / totalAmount), 4);    //Средняя цена покупки
                oldTicker.AmountCrypto = totalAmount;
                oldTicker.SumCrypto = oldTicker.Price * oldTicker.AmountCrypto;
                oldTicker.SumCryptoToRuble = oldTicker.SumCrypto * rate;
                oldTicker.DateAddStock = DateTime.UtcNow;

                _context.Cryptos.Update(oldTicker);
            }
            //Иначе добавляем новую крипту
            else
            {                           
                await _context.Cryptos.AddAsync(crypto);
            }
            
            await _context.SaveChangesAsync();  // Асинхронно сохраняем изменения в БД
        }
        public async Task Delete(int id)    //Удаление криптовалюты
        {
            var crypto = await _context.Cryptos.FindAsync(id);
            if(crypto != null)
            {
                _context.Cryptos.Remove(crypto);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Crypto?> GetAssetById(int id)  //получение криптовалюты для удаления
        {
            return await _context.Cryptos.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<Crypto>> GetAssetsByID(int userId)     //Перечисление всей крипты пользователя
        {
            var crypto = await _context.Cryptos
                .Where(s => s.UserId == userId)
                .ToListAsync(); // Получение таблицы с криптой пользователя           
            return crypto;
        }      

        public async Task<IEnumerable<ForChart>> GetChartTicker(int userId) //График по криптовалюте
        {
            var data = await _context.Cryptos
                .Where(s => s.UserId == userId)
                .GroupBy(e => e.Ticker)
                .Select(g => new ForChart
                {
                    Label = g.Key,
                    Total = g.Sum(e => e.SumCryptoToRuble)
                })
                .ToListAsync();
            return data;
        }
        public async Task FixOldCryptos()   // Для правок в БД
        {
            var cryptos = await _context.Cryptos.ToListAsync();
            decimal rate = await _assetdata.GetCurrencyRate("USD");

            foreach (var crypto in cryptos)
            {
                crypto.SumCrypto = crypto.Price * crypto.AmountCrypto;
                crypto.SumCryptoToRuble = crypto.SumCrypto * rate;
            }

            await _context.SaveChangesAsync();
        }



        public async Task<IEnumerable<Crypto>> GetAll()
        {
            var crypto = await _context.Cryptos.ToListAsync();  // Перечисление всех данных из БД
            return crypto;
        }
    }
}
