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
        private readonly IAssetData _assetdata; // Для парсинга различных курсов

        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
        public StocksController(IStocksService stocksService, IAssetData assetdata)
        {
            _stocksService = stocksService;
            _assetdata = assetdata;
        }

        public async Task<IActionResult> IndexStocks()    // Список всех акций
        {
            var stocks = await _stocksService.GetAssetsByID(CurrentUserId);  // Перечисление всех данных из БД
                        
            return View(stocks);
        }
        public IActionResult Create()   // Страница добавления акции
        {
            return View("CreateStock");
        }
        [HttpPost]
        public async Task<IActionResult> Create(Stock stock)
        {
            stock.UserId = CurrentUserId;  //Привязка к текущему пользователю

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
            if (stock == null || stock.UserId != CurrentUserId)    //Проверка на акции текущего пользователя
                return NotFound();
            return View("DeleteStock",stock);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stock = await _stocksService.GetAssetById(id);
            if (stock == null || stock.UserId != CurrentUserId)    //Проверка на акции текущего пользователя
                return NotFound();
            await _stocksService.Delete(id);
            return RedirectToAction("IndexStocks");
        }
        public async Task<IActionResult> GetChartT()
        {
            var data = await _stocksService.GetChartTicker(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetPurchaseSUM()
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
