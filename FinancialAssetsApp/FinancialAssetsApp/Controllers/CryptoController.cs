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
    public class CryptoController : Controller
    {
        private readonly ICryptosService _cryptosService;
        private readonly HttpClient _httpClient;
        private readonly IAssetData _assetdata;
        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
        public CryptoController(ICryptosService cryptosService, HttpClient httpClient, IAssetData assetdata) 
        {
            _cryptosService = cryptosService;
            _httpClient = httpClient;
            _assetdata = assetdata;
        }
        public async Task<IActionResult> TickersCrypto(string symbol)   // Fetch crypto ticker list from Bybit
        {
            var tickers = await _assetdata.GetTickersCrypto(symbol);
            return Json(tickers);
        }

        public async Task<IActionResult> PriceCrypto(string symbol)    // Get current crypto price
        {
            var price = await _assetdata.GetPriceCrypto(symbol);
            return Json(price);
        }

        public async Task<IActionResult> IndexCrypto()    // List all crypto assets for current user
        {
            var cryptos = await _cryptosService.GetAssetsByID(CurrentUserId);
            //await FixCrypto();    // For manual DB corrections
            return View(cryptos);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Crypto crypto)
        {
            crypto.UserId = CurrentUserId;
            if (!ModelState.IsValid)
            {
                return View(crypto);
            }

            await _cryptosService.Add(crypto);
            return RedirectToAction("IndexCrypto");
        }
        public async Task<IActionResult> Delete(int id)
        {
            var crypto = await _cryptosService.GetAssetById(id);
            if (crypto == null || crypto.UserId != CurrentUserId)    // Verify asset belongs to current user
                return NotFound();
            return View("DeleteCrypto", crypto);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var crypto = await _cryptosService.GetAssetById(id);
            if (crypto == null || crypto.UserId != CurrentUserId)    // Verify asset belongs to current user
                return NotFound();
            await _cryptosService.Delete(id);
            return RedirectToAction("IndexCrypto");
        }
        public async Task<IActionResult> GetChartTicker()
        {
            var data = await _cryptosService.GetChartTicker(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetChangeSUM() // Get difference between the current and invested sum
        {
            var current = await _cryptosService.GetCurrentCryptoSUM(CurrentUserId);
            var purchase = await _cryptosService.GetPurchaseCryptoSUM(CurrentUserId);
            var change = current - purchase;
            return Json(change);
        }
        public async Task<IActionResult> GetChangePercentageSUM()   // Get difference in percentage
        {
            var current = await _cryptosService.GetCurrentCryptoSUM(CurrentUserId);
            var purchase = await _cryptosService.GetPurchaseCryptoSUM(CurrentUserId);
            var changePercent = (((current - purchase) / purchase) * 100);
            return Json(changePercent);
        }
        public async Task<IActionResult> GetCurrentSUM()    // Get current sum
        {
            var current = await _cryptosService.GetCurrentCryptoSUM(CurrentUserId);
            return Json(current);
        }






        /*public async Task<IActionResult> FixCrypto()
        {
            await _cryptosService.FixOldCryptos();
            return RedirectToAction("IndexStocks");
        }*/
    }
}
