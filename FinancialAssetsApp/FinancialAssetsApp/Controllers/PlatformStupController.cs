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
    public class PlatformStupController : Controller
    {
        private readonly IPlatformStartupService _platformService;
        private readonly IAssetData _assetdata; // Для парсинга различных курсов
        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
        public PlatformStupController(IPlatformStartupService platformService, IAssetData assetdata)
        {
            _platformService = platformService;
            _assetdata = assetdata;
        }

        public async Task<IActionResult> Index()    // Список всех платформ
        {
            var platformStartups = await _platformService.GetAssetsByID(CurrentUserId);  // Перечисление всех данных из БД
            return View("IndexPlatform", platformStartups);
        }
        private void FillListPlatforms()    // Метод для списка платформ
        {
            ViewBag.Platforms = new List<SelectListItem>        // Создание списка для выбора платформы
            {
                new SelectListItem {Value = "BrainBox", Text = "BrainBox"},
                new SelectListItem {Value = "Zorko", Text = "Zorko"},
                new SelectListItem {Value = "Поток", Text = "Поток"},
                new SelectListItem {Value = "Bizmall", Text = "Bizmall"},
                new SelectListItem {Value = "Zapusk", Text = "Zapusk"},
                new SelectListItem {Value = "Finmuster", Text = "Finmuster"},
                new SelectListItem {Value = "Rounds", Text = "Rounds"}
            };
        }
        public IActionResult Create()   // Страница добавления акции
        {

            FillListPlatforms();
            return View("CreatePlatform");
        }
        [HttpPost]
        public async Task<IActionResult> Create(PlatformStartup platform)
        {
            platform.UserId = CurrentUserId;  //Привязка к текущему пользователю
            
            if (!ModelState.IsValid)
            {
                FillListPlatforms();
                return View("CreatePlatform", platform);
            }
            await _platformService.Add(platform);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(int id)
        {
            
            var platform = await _platformService.GetAssetById(id);
            if (platform == null || platform.UserId != CurrentUserId)    //Проверка на платформу
                return NotFound();
            return View("DeletePlatform", platform);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var platform = await _platformService.GetAssetById(id);
            if (platform == null || platform.UserId != CurrentUserId)    //Проверка на акции текущего пользователя
                return NotFound();
            await _platformService.Delete(id);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> GetChartT()
        {
            var data = await _platformService.GetChartTicker(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetChartCountComp()
        {
            var data = await _platformService.GetChartCount(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetPurchaseSUM()    // Get current sum
        {
            var current = await _platformService.GetPurchasePlStartupsSUM(CurrentUserId);
            return Json(current);
        }



        /*public async Task<IActionResult> FixSums()
        {
            await _stocksService.FixOldStocks();
            return RedirectToAction("IndexStocks");
        }*/
    }
}
