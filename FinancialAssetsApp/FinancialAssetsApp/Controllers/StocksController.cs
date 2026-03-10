using FinancialAssetsApp.Data;
using FinancialAssetsApp.Data.Service;
using FinancialAssetsApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace FinancialAssetsApp.Controllers
{
    public class StocksController : Controller
    {
        private readonly IStocksService _stocksService;
        private readonly IAssetData _assetdata; // For fetching various exchange rates
        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
        public StocksController(IStocksService stocksService, IAssetData assetdata)
        {
            _stocksService = stocksService;
            _assetdata = assetdata;
        }
        public async Task<IActionResult> IndexStocks()    // List all stock assets
        {
            var stocks = await _stocksService.GetAssetsByID(CurrentUserId);  // Fetch all records from DB
            return View(stocks);
        }
        public IActionResult Create()   // Show the add stock page
        {
            return View("CreateStock");
        }
        [HttpPost]
        public async Task<IActionResult> Create(Stock stock)
        {
            stock.UserId = CurrentUserId;  // Bind asset to the current user
            if (!ModelState.IsValid)
            {
                return View("CreateStock", stock);
            }
            await _stocksService.Add(stock);
            return RedirectToAction("IndexStocks");
        }
        public async Task<IActionResult> Delete(int id)
        {
            var stock = await _stocksService.GetAssetById(id);
            if (stock == null || stock.UserId != CurrentUserId)    // Verify asset belongs to current user
                return NotFound();
            return View("DeleteStock", stock);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stock = await _stocksService.GetAssetById(id);
            if (stock == null || stock.UserId != CurrentUserId)    // Verify asset belongs to current user
                return NotFound();
            await _stocksService.Delete(id);
            return RedirectToAction("IndexStocks");
        }
        public async Task<IActionResult> GetChartT() // Chart for stocks by current sum
        {
            var data = await _stocksService.GetChartTicker(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetPurchaseSUM()   // Chart for stocks by invested sum
        {
            var data = await _stocksService.GetPurchaseStocksSUM(CurrentUserId);
            return Json(data);
        }



        /*public async Task<IActionResult> GetChartC()
        {
            var data = await _stocksService.GetChartCountry(CurrentUserId);
            return Json(data);
        }*/






        /*public async Task<IActionResult> FixSums()
        {
            await _stocksService.FixOldStocks();
            return RedirectToAction("IndexStocks");
        }*/
    }
}
