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
        private readonly FinanceDbContext _context; // БД
        public AssetData(HttpClient httpClient, FinanceDbContext context)
        {
            _context = context;
            _httpClient = httpClient;
        }
        public async Task<decimal> GetCurrencyRate(string code)    // Получение курса валюты
        {
            var dataAsset = await _httpClient.GetStringAsync("https://www.cbr-xml-daily.ru/daily_json.js");
            var doc = JsonDocument.Parse(dataAsset);

            var currency = doc.RootElement.GetProperty("Valute");

            if (currency.TryGetProperty(code, out var rateInfo))
                return rateInfo.GetProperty("Value").GetDecimal();

            throw new Exception($"Валюта {code} не найдена");
        }
        public async Task<string> GetCurrencyCode(string nameCurrency)   //Получение кода для валюты
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
        public async Task<List<string>> GetTickersCrypto(string symbol)   //Получение списка крипта с Bybit
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
        public async Task<List<string>> GetCitiesList (string symbol)   // Получение списка городов России
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
        public async Task<decimal> GetPriceCrypto(string symbol)    //Получение текущей цены крипты
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
        public async Task<decimal> GetMetalPrice(string nameMetal)    // Получение курса металла
        {
            DateTime date = DateTime.Today; //Для актуальной даты для парсинга
            string day = date.ToString("dd");
            string month = date.ToString("MM");
            string year = date.ToString("yyyy");    

            int flag = 0;
            while (flag == 0)   // Пока не найдена последняя цена металла, не выходим из цикла
            {
                string url = $"https://www.cbr.ru/scripts/xml_metall.asp?date_req1={day}/{month}/{year}&date_req2={day}/{month}/{year}";

                // читаем байты, а не строку
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); //Проверка на успешный ответ

                var bytes = await response.Content.ReadAsByteArrayAsync();  // Считывание байтов
                var dataAsset = Encoding.GetEncoding("windows-1251").GetString(bytes);  //Преобразование в строку в нужной кодировке

                var doc = XDocument.Parse(dataAsset);   //Парсит XML в объект XDocument, тк данные на сайте ЦБ РФ в xml
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
                    .FirstOrDefault(r => r.Attribute("Code")?.Value == code);   //Находим элемент с нужным кодом
                if (record == null) // если данных нет, берем предыдущий день. 
                {                   // По воскресеньям и понедельникам цен на металлы нет
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
                    var sell = record.Element("Sell")!.Value.Replace(',', '.'); // Берется цена продажи металла и меняется запятая на точку для распознавания числа
                    return decimal.Parse(sell, NumberStyles.Any, CultureInfo.InvariantCulture);
                }                 
            }
            return 0;           
        }
        public async Task<decimal> RUgetStockPrice(string ticker)   // Получение курса акции
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
