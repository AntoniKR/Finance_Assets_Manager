namespace FinancialAssetsApp.Data.Service
{
    public interface IAssetData     // Interface for fetching live prices of various asset types
    {
        Task<decimal> GetCurrencyRate(string code);    // Get currency exchange rate
        Task<List<string>> GetTickersCrypto(string symbols);    // Get crypto ticker list for asset selection
        Task<decimal> GetPriceCrypto(string symbol);    // Get current crypto price
        Task<decimal> GetMetalPrice(string code);    // Get metal price
        Task<decimal> RUgetStockPrice(string ticker);    // Get RUB stock price
        Task<string> GetCurrencyCode(string symbol); // Get currency code list from CBR
        Task<List<string>> GetCitiesList(string symbol);    // Get list of Russian cities
    }
}
