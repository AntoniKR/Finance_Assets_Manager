using FinancialAssetsApp.Data;
using FinancialAssetsApp.Data.Service;
using FinancialAssetsApp.Models;
using FinancialAssetsApp.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Net.Http;

namespace FinancialAssetsApp.Controllers
{
    public class CurrencyController : Controller
    {
        private readonly ICurrenciesService _currenciesService;
        private readonly HttpClient _httpClient;
        private readonly IAssetData _assetdata;
        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
        public CurrencyController(ICurrenciesService currencyService, HttpClient httpClient, IAssetData assetdata) 
        {
            _currenciesService = currencyService;
            _httpClient = httpClient;
            _assetdata = assetdata;
        }
      
        public async Task<IActionResult> PriceCurrency (string symbol)    //Получение текущей цены валюты
        {
            var price = await _assetdata.GetCurrencyRate(symbol);
            return Json(price);
        }
        public async Task<IActionResult> IndexCurrency()    // Список всей валюты
        {          
            var currencies = await _currenciesService.GetAssetsByID(CurrentUserId);           
            return View("IndexCurrency", currencies);
            //await FixCrypto();    // Для правок в БД
        }
        public IActionResult CreateCurrency()
        {
            return View("CreateCurrency");
        }
        [HttpPost]
        public async Task<IActionResult> CreateCurrency(Currency currency)
        {
            currency.UserId = CurrentUserId;
            if (!ModelState.IsValid)
            {
                return View("CreateCurrency",currency);
            }

            await _currenciesService.Add(currency);
            return RedirectToAction("IndexCurrency");
        }
        public async Task<IActionResult> DeleteCurrency(int id)
        {
            var currency = await _currenciesService.GetAssetById(id);
            if (currency == null || currency.UserId != CurrentUserId)    //Проверка на акции текущего пользователя
                return NotFound();
            return View("DeleteCurrency", currency);
        }
        [HttpPost, ActionName("DeleteCurrency")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currency = await _currenciesService.GetAssetById(id);
            if (currency == null || currency.UserId != CurrentUserId)    //Проверка на акции текущего пользователя
                return NotFound();
            await _currenciesService.Delete(id);
            return RedirectToAction("IndexCurrency");
        }
        public async Task<IActionResult> GetChartTicker()
        {
            var data = await _currenciesService.GetChartTicker(CurrentUserId);
            return Json(data);
        }
        [HttpGet]
        public async Task<IActionResult> GetCurrencies(string nameCurrency)
        {
            var currenciesList = await _assetdata.GetCurrencyCode(nameCurrency);
            return Json(currenciesList);
        }
        public async Task<IActionResult> GetChangeSUM() // Get difference between the current and invested sum
        {
            var current = await _currenciesService.GetCurrentCurrenciesSUM(CurrentUserId);
            var purchase = await _currenciesService.GetPurchaseCurrenciesSUM(CurrentUserId);
            var change = current - purchase;
            return Json(change);
        }
        public async Task<IActionResult> GetChangePercentageSUM()   // Get difference in percentage
        {
            var current = await _currenciesService.GetCurrentCurrenciesSUM(CurrentUserId);
            var purchase = await _currenciesService.GetPurchaseCurrenciesSUM(CurrentUserId);
            var changePercent = (((current - purchase) / purchase) * 100);
            return Json(changePercent);
        }
        public async Task<IActionResult> GetCurrentSUM()    // Get current sum
        {
            var current = await _currenciesService.GetCurrentCurrenciesSUM(CurrentUserId);
            return Json(current);
        }
        /*public async Task<IActionResult> FixCrypto()
        {
            await _cryptosService.FixOldCryptos();
            return RedirectToAction("IndexStocks");
        }*/
    }
}
