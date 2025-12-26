using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FinancialAssetsApp.Data.Service
{
    public class HomeService
    {
        private readonly FinanceDbContext _context; // БД
        private readonly IAssetData _assetdata; // Для парсинга различных курсов
        private readonly IStocksUSDService _stockUSD;

        public HomeService(FinanceDbContext context, IAssetData assetdata, IStocksUSDService stockUSD)  // Конструктор
        {
            _context = context;
            _assetdata = assetdata;
            _stockUSD = stockUSD;
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
        public async Task<decimal> GetCurrentCryptoSUM(int userId)    // Получение текущего курса crypto
        {
            var cryptos = await _context.Cryptos
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal totalCurrSum = 0;
            var usdRate = await _assetdata.GetCurrencyRate("USD");
            foreach (var crypto in cryptos)
            {
                var price = await _assetdata.GetPriceCrypto(crypto.Ticker);
                totalCurrSum += (crypto.AmountCrypto * price);                
            }
            totalCurrSum *= usdRate;
            return totalCurrSum;
        }
        public async Task<decimal?> GetCurrentStartupsSUM(int userId)    // Получение текущего курса Startups
        {
            var platformStps = await _context.PlatformStartups
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal? totalCurrSum = 0;
            foreach (var startups in platformStps)
            {
                totalCurrSum += startups.SumOfStartups;
            }
            return totalCurrSum;
        }
        public async Task<decimal> GetCurrentMetalSUM(int userId)    // Получение текущего курса Metals
        {
            var metals = await _context.Metals
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal totalCurrSum = 0;
            foreach (var metal in metals)
            {
                var price = await _assetdata.GetMetalPrice(metal.NameMetal);
                totalCurrSum += (metal.AmountMetal * price);
            }
            return totalCurrSum;
        }
        public async Task<decimal> GetCurrentRUSStocksSUM(int userId)    // Получение текущего курса RUSStocks
        {
            var ruStocks = await _context.Stocks
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal totalCurrSum = 0;
            foreach (var stock in ruStocks)
            {
                totalCurrSum += stock.SumStocks;
            }
            return totalCurrSum;
        }
        public async Task<decimal> Get(int userId)    // Получение текущего курса RUSStocks
        {
            var ruStocks = await _context.Stocks
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal totalCurrSum = 0;
            foreach (var stock in ruStocks)
            {
                totalCurrSum += stock.SumStocks;
            }
            return totalCurrSum;
        }
        public async Task<decimal> GetCurrentCurrenciesSUM(int userId)    // Получение текущего курса crypto
        {
            var currencies = await _context.Currencies
                .Where(s => s.UserId == userId)
                .ToListAsync();

            decimal totalCurrSum = 0;
            var usdRate = await _assetdata.GetCurrencyRate("USD");
            foreach (var currency in currencies)
            {
                var price = await _assetdata.GetCurrencyRate(currency.CharCode);
                totalCurrSum += (currency.AmountCurrency * price);
            }
            return totalCurrSum;
        }
        public async Task<decimal> GetCurrentAss(int userId)    // Получение текущего курса всего
        {
            decimal totalCurrSum = 0;

            /*var cryptos = await GetCurrentCryptoSUM(userId);
            var stps = await GetCurrentStartupsSUM(userId);
            var startups = stps.Value;
            var metals = await GetCurrentMetalSUM(userId);
            var ruStocks = await GetCurrentRUSStocksSUM(userId);
            //var usStocks = await GetCurrentUSStocksSUM(userId);
            //var currencies = await GetCurrentCurrenciesSUM(userId);
            Console.WriteLine($"QQQQQQQQQQQQQQQQQQQQQQQQQQQQQ {cryptos}, {metals}, {startups}, {ruStocks} ");
            totalCurrSum += (cryptos + startups + metals + ruStocks);*/
            //var usdRate = await _assetdata.GetCurrencyRate("USD");
            totalCurrSum += await _stockUSD.GetCurrentUSStocksSUM(userId);
            return totalCurrSum;
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
