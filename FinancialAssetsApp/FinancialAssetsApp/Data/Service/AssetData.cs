using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace FinancialAssetsApp.Data.Service
{
    public class AssetData : IAssetData
    {
        private readonly HttpClient _httpClient;
        private readonly FinanceDbContext _context; // Database context
        public AssetData(HttpClient httpClient, FinanceDbContext context)
        {
            _context = context;
            _httpClient = httpClient;
        }
        public async Task<decimal> GetCurrencyRate(string code)    // Fetch currency exchange rate from CBR
        {
            var dataAsset = await _httpClient.GetStringAsync("https://www.cbr-xml-daily.ru/daily_json.js");
            var doc = JsonDocument.Parse(dataAsset);

            var currency = doc.RootElement.GetProperty("Valute");

            if (currency.TryGetProperty(code, out var rateInfo))
                return rateInfo.GetProperty("Value").GetDecimal();

            throw new Exception($"Валюта {code} не найдена");
        }
        public async Task<string> GetCurrencyCode(string nameCurrency)   // Get currency code by name
        {
            var dataAsset = await _httpClient.GetStringAsync("https://www.cbr-xml-daily.ru/daily_json.js");
            var doc = JsonDocument.Parse(dataAsset);
            var currency = doc.RootElement.GetProperty("Valute");

            foreach(var item in currency.EnumerateObject())
            {
                var charCode = item.Value;
                if(charCode.GetProperty("Name").GetString() == nameCurrency)
                    return charCode.GetProperty("CharCode").GetString();

            }
            throw new Exception($"Код валюты {nameCurrency} не найден");

        }
        public async Task<List<string>> GetTickersCrypto(string symbol)   // Fetch crypto ticker list from Bybit
        {
            var urlTickers = "https://api.bybit.com/v5/market/tickers?category=spot";
            var response = await _httpClient.GetStringAsync(urlTickers);
            var json = JsonDocument.Parse(response);

            var tickers = json.RootElement
                .GetProperty("result")
                .GetProperty("list")
                .EnumerateArray()
                .Select(x => x.GetProperty("symbol").GetString())
                .Where(s => string.IsNullOrEmpty(symbol) || s.StartsWith(symbol.ToUpper()))
                .Take(20)
                .ToList();
            return tickers;
        }
        public async Task<List<string>> GetCitiesList (string symbol)   // Fetch list of Russian cities
        {
            var urlCities = "https://raw.githubusercontent.com/pensnarik/russian-cities/master/russian-cities.json";
            var response = await _httpClient.GetStringAsync(urlCities);
            var json = JsonDocument.Parse(response);

            var cities = json.RootElement
                .EnumerateArray()
                .Select(x => x.GetProperty("name").GetString())
                .Where(s => string.IsNullOrEmpty(symbol) || 
                    s.Contains(symbol, StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .ToList();
            return cities;

        }
        public async Task<decimal> GetPriceCrypto(string symbol)    // Get current crypto price from Bybit
        {
            var urlTickers = "https://api.bybit.com/v5/market/tickers?category=spot";
            var response = await _httpClient.GetStringAsync(urlTickers);
            var json = JsonDocument.Parse(response);

            var ticker = json.RootElement
                .GetProperty("result")
                .GetProperty("list")
                .EnumerateArray()
                .FirstOrDefault(x => x.GetProperty("symbol").GetString() == symbol);

            if (ticker.ValueKind == JsonValueKind.Undefined)
                return 0;

            var price = ticker.GetProperty("lastPrice").GetString();
            return decimal.Parse(price, CultureInfo.InvariantCulture);

        }
        public async Task<decimal> GetMetalPrice(string nameMetal)    // Fetch metal price from CBR
        {
            DateTime date = DateTime.Today; // Use today's date for initial request
            string day = date.ToString("dd");
            string month = date.ToString("MM");
            string year = date.ToString("yyyy");

            int flag = 0;
            while (flag == 0)   // Keep looping until the latest metal price is found
            {
                string url = $"https://www.cbr.ru/scripts/xml_metall.asp?date_req1={day}/{month}/{year}&date_req2={day}/{month}/{year}";

                // Read raw bytes instead of string to handle encoding correctly
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Verify successful response

                var bytes = await response.Content.ReadAsByteArrayAsync();  // Read response as bytes
                var dataAsset = Encoding.GetEncoding("windows-1251").GetString(bytes);  // Decode from Windows-1251 encoding

                var doc = XDocument.Parse(dataAsset);   // Parse XML response from CBR into XDocument
                string code = "0";
                switch (nameMetal)
                {
                    case "Золото":
                        code = "1";
                        break;
                    case "Серебро":
                        code = "2";
                        break;
                    case "Платина":
                        code = "3";
                        break;
                    case "Палладий":
                        code = "4";
                        break;
                    default:
                        break;
                }

                var record = doc.Descendants("Record")
                    .FirstOrDefault(r => r.Attribute("Code")?.Value == code);   // Find the record with the matching metal code
                if (record == null) // If no data for this date, fall back to the previous day
                {                   // CBR does not publish metal prices on Sundays and Mondays
                    if (int.Parse(day) != 1)
                    {
                        int temp = int.Parse(day) - 1;
                        day = temp.ToString();
                    }
                    else if (int.Parse(month) == 1)
                    {
                        int temp = 12;
                        month = temp.ToString();
                        temp = 31;
                        day = temp.ToString();
                        temp = 1;
                        year = (int.Parse(year) - temp).ToString();
                    }
                    else
                    {
                        int temp = 1;
                        month = (int.Parse(month) - temp).ToString();
                        temp = 28;
                        day = temp.ToString();
                    }
                    continue;
                }
                else
                {
                    var sell = record.Element("Sell")!.Value.Replace(',', '.'); // Use sell price and replace comma with dot for decimal parsing
                    return decimal.Parse(sell, NumberStyles.Any, CultureInfo.InvariantCulture);
                }
            }
            return 0;
        }
        public async Task<decimal> RUgetStockPrice(string ticker)   // Fetch Russian stock price from MOEX (on working)
        {
            try
            {
                var urlTicker = $"https://iss.moex.com/iss/engines/stock/markets/shares/securities/{ticker}.json";
                var response = await _httpClient.GetStringAsync(urlTicker);
                using var doc = JsonDocument.Parse(response);

                var rows = doc.RootElement
                    .GetProperty("marketdata")
                    .GetProperty("data");

                if (rows.GetArrayLength() == 0)
                    return 0;
                var row = rows[0];

                decimal price = 0;

                if (row[4].ValueKind != JsonValueKind.Null)
                    price = row[4].GetDecimal();
                else if (row[2].ValueKind != JsonValueKind.Null)
                    price = row[2].GetDecimal();

                return price;
            }
            catch
            {
                return 0;
            }

        }

    }
}
