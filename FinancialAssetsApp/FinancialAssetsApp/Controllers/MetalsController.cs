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
    public class MetalsController : Controller
    {
        private readonly IMetalsService _metalService;
        private readonly IAssetData _assetdata; // Для парсинга различных курсов
        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
        public MetalsController(IMetalsService metalService, IAssetData assetdata)
        {
            _metalService = metalService;
            _assetdata = assetdata;
        }

        public async Task<IActionResult> IndexMetals()    // Список всех акций
        {
            var metals = await _metalService.GetAssetsByID(CurrentUserId);  // Перечисление всех данных из БД
            return View("IndexMetals", metals);
        }
        private void FillListMetals()    // Метод для списка металлов 
        {
            ViewBag.Metals = new List<SelectListItem>        // Создание списка для выбора металла
            {
                new SelectListItem {Value = "Золото", Text = "Золото"},
                new SelectListItem {Value = "Серебро", Text = "Серебро"},
                new SelectListItem {Value = "Палладий", Text = "Палладий"},
                new SelectListItem {Value = "Платина", Text = "Платина"}
            };
        }
        public IActionResult Create()   // Страница добавления акции
        {
            
            FillListMetals();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Metal metal)
        {
            metal.UserId = CurrentUserId;  //Привязка к текущему пользователю
            
            if (!ModelState.IsValid)
            {
                FillListMetals();
                return View(metal);
            }
            await _metalService.Add(metal);
            return RedirectToAction("IndexMetals");
        }
        public async Task<IActionResult> Delete(int id)
        {
            
            var metal = await _metalService.GetAssetById(id);
            if (metal == null || metal.UserId != CurrentUserId)    //Проверка на акции текущего пользователя
                return NotFound();
            return View("DeleteMetal", metal);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var metal = await _metalService.GetAssetById(id);
            if (metal == null || metal.UserId != CurrentUserId)    //Проверка на акции текущего пользователя
                return NotFound();
            await _metalService.Delete(id);
            return RedirectToAction("IndexMetals");
        }
        public async Task<IActionResult> GetChartT()
        {
            var data = await _metalService.GetChartTicker(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> PriceMetal(string nameMetal)   //Получение цены на металлы
        {
            var price = await _assetdata.GetMetalPrice(nameMetal);
            return Json(price);
        }
        public async Task<IActionResult> GetChangeSUM() // Get difference between the current and invested sum
        {
            var current = await _metalService.GetCurrentMetalsSUM(CurrentUserId);
            var purchase = await _metalService.GetPurchaseMetalsSUM(CurrentUserId);
            var change = current - purchase;
            return Json(change);
        }
        public async Task<IActionResult> GetChangePercentageSUM()   // Get difference in percentage
        {
            var current = await _metalService.GetCurrentMetalsSUM(CurrentUserId);
            var purchase = await _metalService.GetPurchaseMetalsSUM(CurrentUserId);
            var changePercent = (((current - purchase) / purchase) * 100);
            return Json(changePercent);
        }
        public async Task<IActionResult> GetCurrentSUM()    // Get current sum
        {
            var current = await _metalService.GetCurrentMetalsSUM(CurrentUserId);
            return Json(current);
        }


        /*public async Task<IActionResult> FixSums()
        {
            await _stocksService.FixOldStocks();
            return RedirectToAction("IndexStocks");
        }*/
    }
}
