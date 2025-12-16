using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FinancialAssetsApp.Data.Service
{
    public class HomeService
    {
        private readonly FinanceDbContext _context; // БД
        private readonly IAssetData _assetdata; // Для парсинга различных курсов

        public HomeService(FinanceDbContext context, IAssetData assetdata)  // Конструктор
        {
            _context = context;
            _assetdata = assetdata;
        }
        public async Task<IEnumerable<ForChart>> GetAssetsSumm (int userId)
        {
            var totalStocks = await _context.Stocks
                .Where(s => s.UserId == userId)
                .SumAsync(e => e.SumStocks);
            var totalStocksUSD = await _context.StocksUSD
                .Where(s => s.UserId == userId)
                .SumAsync(e => e.SumStocksToRuble);
            var totalCrypto = await _context.Cryptos
                .Where(s => s.UserId == userId)
                .SumAsync(e => e.SumCryptoToRuble);
            var totalMetals = await _context.Metals
                .Where(s => s.UserId == userId)
                .SumAsync(e => e.SumMetals);
            var totalCurrencies = await _context.Currencies
                .Where(s => s.UserId == userId)
                .SumAsync(e => e.SumCurrencyToRuble);
            var totalStartups = await _context.Startups
                .Where(s => s.UserId == userId)
                .SumAsync(e => e.SumStocks);

            return new List<ForChart>
            {
                new ForChart{Label = "Акции ₽", Total = totalStocks},
                new ForChart{Label = "Акции $", Total = totalStocksUSD},
                new ForChart{Label = "Криптовалюта", Total = totalCrypto},
                new ForChart{Label = "Металлы", Total = totalMetals},
                new ForChart{Label = "Валюта", Total = totalCurrencies},
                new ForChart{Label = "Стартапы", Total = totalStartups},
            };
        }

        public async Task<IEnumerable<ForChart>> GetEstateTransSumm(int userId)
        {
            var totalEstate = await _context.RealEstates
                .Where(s => s.UserId == userId)
                .SumAsync(e => e.SumEstate);
            var totalTrans = await _context.Transports
                .Where(s => s.UserId == userId)
                .SumAsync(e => e.Price);

            return new List<ForChart>
            {
                new ForChart{Label = "Недвижимость", Total = totalEstate},
                new ForChart{Label = "Транспорт", Total = totalTrans}
            };
        }

        public async Task<decimal> GetRate()
        {
            return await _assetdata.GetCurrencyRate("USD");
        }
    }
}
