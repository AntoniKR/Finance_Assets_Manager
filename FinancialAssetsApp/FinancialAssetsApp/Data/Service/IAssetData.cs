namespace FinancialAssetsApp.Data.Service
{
    public interface IAssetData     //Интерфейс для курса различных активов
    {
        Task<decimal> GetCurrencyRate(string code);    //курс валют
        Task<List<string>> GetTickersCrypto(string symbols);    //Список крипты для добавления 
        Task<decimal> GetPriceCrypto(string symbol);    //Получение текущей цены крипты
        Task<decimal> GetMetalPrice(string code);    // курс металлов
        Task<decimal> RUgetStockPrice(string ticker);    // курс металлов
        Task<string> GetCurrencyCode(string symbol); // Список валюты с ЦБ РФ
        Task<List<string>> GetCitiesList(string symbol);    // Список городов России

    }
}
