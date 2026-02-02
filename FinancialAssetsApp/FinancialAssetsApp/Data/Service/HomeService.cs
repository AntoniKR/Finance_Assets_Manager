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
        private readonly IStocksService _stocks;
        private readonly IStocksUSDService _stocksUSD;
        private readonly ICryptosService _cryptos;
        private readonly IMetalsService _metals;
        private readonly ICurrenciesService _currencies;
        private readonly IPlatformStartupService _startups;

        public HomeService(FinanceDbContext context, IAssetData assetdata, IStocksService stocksService,IStocksUSDService stockUSD, ICryptosService cryptosService, IMetalsService metalsService, ICurrenciesService currenciesService, IPlatformStartupService startupService)  // Конструктор
        {
            _context = context;
            _assetdata = assetdata;
            _stocks = stocksService;
            _stocksUSD = stockUSD;
            _cryptos = cryptosService;
            _metals = metalsService;
            _currencies = currenciesService;
            _startups = startupService;
        }
        public async Task<decimal> GetPurchaseTotal(int userId)    // Get Purchase total sum
        {
            decimal totalPurchaseSum = 0;
            totalPurchaseSum += await _stocks.GetPurchaseStocksSUM(userId);
            totalPurchaseSum += await _stocksUSD.GetPurchaseUSStocksSUM(userId);
            totalPurchaseSum += await _cryptos.GetPurchaseCryptoSUM(userId);
            totalPurchaseSum += await _metals.GetPurchaseMetalsSUM(userId);
            totalPurchaseSum += await _currencies.GetPurchaseCurrenciesSUM(userId);            
            totalPurchaseSum += await _startups.GetPurchasePlStartupsSUM(userId);

            return totalPurchaseSum;
        }
        public async Task<IEnumerable<ForChart>> GetAssetsSumInvested(int userId)   // For pie chart
        {
            var totalStocks = await _stocks.GetPurchaseStocksSUM(userId);
            var totalStocksUSD = await _stocksUSD.GetPurchaseUSStocksSUM(userId);
            var totalCrypto = await _cryptos.GetPurchaseCryptoSUM(userId);
            var totalMetals = await _metals.GetPurchaseMetalsSUM(userId);
            var totalCurrencies = await _currencies.GetPurchaseCurrenciesSUM(userId);
            //var totalStartups = await _startups.GetPurchasePlStartupsSUM(userId);

            return new List<ForChart>
            {
                new ForChart{Label = "Акции ₽", Total = totalStocks},
                new ForChart{Label = "Акции $", Total = totalStocksUSD},
                new ForChart{Label = "Криптовалюта", Total = totalCrypto},
                new ForChart{Label = "Металлы", Total = totalMetals},
                new ForChart{Label = "Валюта", Total = totalCurrencies},
                //new ForChart{Label = "Стартапы", Total = totalStartups},
            };
        }         
        public async Task<decimal> GetCurrentAss(int userId)    // Get current total sum
        {
            decimal totalCurrSum = 0;
            totalCurrSum += await _stocks.GetPurchaseStocksSUM(userId);

            totalCurrSum += await _stocksUSD.GetCurrentUSStocksSUM(userId);
            totalCurrSum += await _cryptos.GetCurrentCryptoSUM(userId);
            totalCurrSum += await _metals.GetCurrentMetalsSUM(userId);
            totalCurrSum += await _currencies.GetCurrentCurrenciesSUM(userId);
            totalCurrSum += await _startups.GetPurchasePlStartupsSUM(userId);
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
        public async Task<decimal> GetRate()
        {
            return await _assetdata.GetCurrencyRate("USD");
        }
    }
}
