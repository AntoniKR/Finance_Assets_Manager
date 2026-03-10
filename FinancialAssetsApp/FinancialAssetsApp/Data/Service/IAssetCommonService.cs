using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;

namespace FinancialAssetsApp.Data.Service
{
    public interface IAssetCommonService<T> // Common CRUD operations for all asset types
    {
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetAssetsByID(int userId);
        Task<T?> GetAssetById(int userId);
        Task Add(T asset);
        Task Delete(int id);
    }
    public interface IStocksService : IAssetCommonService<Stock> // Chart data by RUB stock ticker
    {
        Task<IEnumerable<ForChart>> GetChartTicker(int userId);
        Task<decimal> GetPurchaseStocksSUM(int userId);
    }
    public interface IStocksUSDService : IAssetCommonService<StockUSD> // Chart data by USD stock ticker
    {
        Task<IEnumerable<ForChart>> GetChartTicker(int userId);
        Task<decimal> GetCurrentUSStocksSUM(int userId);
        Task<decimal> GetPurchaseUSStocksSUM(int userId);
    }
    public interface ICryptosService : IAssetCommonService<Crypto>    // Chart data by crypto ticker
    {
        Task<IEnumerable<ForChart>> GetChartTicker(int userId);
        Task<decimal> GetCurrentCryptoSUM(int userId);
        Task<decimal> GetPurchaseCryptoSUM(int userId);
    }
    public interface IMetalsService : IAssetCommonService<Metal>    // Chart data by metal name
    {
        Task<IEnumerable<ForChart>> GetChartTicker(int userId);
        Task<decimal> GetCurrentMetalsSUM(int userId);
        Task<decimal> GetPurchaseMetalsSUM(int userId);
    }
    public interface ICurrenciesService : IAssetCommonService<Currency>    // Chart data by currency name
    {
        Task<IEnumerable<ForChart>> GetChartTicker(int userId);
        Task<decimal> GetCurrentCurrenciesSUM(int userId);
        Task<decimal> GetPurchaseCurrenciesSUM(int userId);
    }
    public interface IPlatformStartupService : IAssetCommonService<PlatformStartup>
    {
        Task<IEnumerable<ForChart>> GetChartTicker(int userId);
        Task<IEnumerable<ForChart>> GetChartCount(int userId);
        Task<decimal> GetPurchasePlStartupsSUM(int userId);
    }
    public interface IStartupService : IAssetCommonService<Startup>
    {
        Task<int> GetPlatformId(string namePlatform);
        Task<IEnumerable<PlatformStartup>> GetAllPlatforms(int userId);
        Task<IEnumerable<ForChart>> GetChartTicker(int userId);
        Task<IEnumerable<ForChart>> GetChartCount(int userId);
    }
    public interface IRealEstateService : IAssetCommonService<RealEstate>
    {
        Task<IEnumerable<ForChart>> GetChartCities(int userId);
        Task<IEnumerable<ForChart>> GetChartType(int userId);
    }
    public interface ITransportService : IAssetCommonService<Transport>
    {
        Task<IEnumerable<ForChart>> GetChartTypeTrans(int userId);
        Task<IEnumerable<ForChart>> GetChartSumTrans(int userId);
    }
}
