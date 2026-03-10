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

        public async Task<IActionResult> PriceCurrency(string symbol)    // Get current currency exchange rate
        {
            var price = await _assetdata.GetCurrencyRate(symbol);
            return Json(price);
        }
        public async Task<IActionResult> IndexCurrency()    // List all currency assets for current user
        {
            var currencies = await _currenciesService.GetAssetsByID(CurrentUserId);
            return View("IndexCurrency", currencies);
            //await FixCrypto();    // For manual DB corrections
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
            if (currency == null || currency.UserId != CurrentUserId)    // Verify asset belongs to current user
                return NotFound();
            return View("DeleteCurrency", currency);
        }
        [HttpPost, ActionName("DeleteCurrency")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currency = await _currenciesService.GetAssetById(id);
            if (currency == null || currency.UserId != CurrentUserId)    // Verify asset belongs to current user
                return NotFound();
            await _currenciesService.Delete(id);
            return RedirectToAction("IndexCurrency");
        }
        public async Task<IActionResult> GetChartTicker()   // // Chart for currencies by invested sum
        {
            var data = await _currenciesService.GetChartTicker(CurrentUserId);
            return Json(data);
        }
        [HttpGet]
        public async Task<IActionResult> GetCurrencies(string nameCurrency)  // List currency 
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
