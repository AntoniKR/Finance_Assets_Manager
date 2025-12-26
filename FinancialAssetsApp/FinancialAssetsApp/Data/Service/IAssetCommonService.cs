using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;

namespace FinancialAssetsApp.Data.Service
{
    public interface IAssetCommonService<T> //Общие действия для активов
    {
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetAssetsByID(int userId);
        Task<T?> GetAssetById(int userId);
        Task Add(T asset);
        Task Delete(int id);

    }
    public interface IStocksService : IAssetCommonService<Stock> // Построение графиков по тикерам рос. акций
    {
        Task<IEnumerable<ForChart>> GetChartTicker(int userId);
        Task<decimal> GetPurchaseStocksSUM(int userId);
    }
    public interface IStocksUSDService : IAssetCommonService<StockUSD> // Построение графиков по тикерам иностранных акций
    {
        Task<IEnumerable<ForChart>> GetChartTicker(int userId);
        Task<decimal> GetCurrentUSStocksSUM(int userId);
        Task<decimal> GetPurchaseUSStocksSUM(int userId);
    }
    public interface ICryptosService : IAssetCommonService<Crypto>    // График по тикерам крипты
    {
        Task<IEnumerable<ForChart>> GetChartTicker(int userId);
        Task<decimal> GetCurrentCryptoSUM(int userId);
        Task<decimal> GetPurchaseCryptoSUM(int userId);


    }
    public interface IMetalsService : IAssetCommonService<Metal>    // График по металлам
    {
        Task<IEnumerable<ForChart>> GetChartTicker(int userId);
        //Task<decimal> GetPurchaseMetalsSUM(int userId);

    }
    public interface ICurrenciesService : IAssetCommonService<Currency>    // График по валютам
    {
        Task<IEnumerable<ForChart>> GetChartTicker(int userId);
        //Task<decimal> GetPurchaseCurrenciesSUM(int userId);

    }
    public interface IPlatformStartupService : IAssetCommonService<PlatformStartup>
    {
        Task<IEnumerable<ForChart>> GetChartTicker(int userId);
        Task<IEnumerable<ForChart>> GetChartCount(int userId);
        //Task<decimal> GetPurchasePlStartupsSUM(int userId);

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
