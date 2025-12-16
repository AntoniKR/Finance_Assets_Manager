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
    public class StartupsController : Controller
    {
        private readonly IStartupService _startupService;
        private readonly IAssetData _assetdata; // Для парсинга различных курсов
        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
        public StartupsController(IStartupService startupService, IAssetData assetdata)
        {
            _startupService = startupService;
            _assetdata = assetdata;
        }

        public async Task<IActionResult> Index()    // Список всех стартапов
        {
            var startup = await _startupService.GetAssetsByID(CurrentUserId);  // Перечисление всех данных из БД
            return View("IndexStartup", startup);
        }
        private async Task FillListPlatforms()    // Метод для заполнения списка добавленных платформ
        {
            var platforms = await _startupService.GetAllPlatforms(CurrentUserId);
            
            ViewBag.PlatformStartup = platforms
                .Select(plms => new SelectListItem
            {
                Value = plms.NamePlatform,
                Text = plms.NamePlatform
            })
                .ToList();

        }
        public async Task<IActionResult> Create()   // Страница добавления акции
        {

            await FillListPlatforms();
            return View("CreateStartup");
        }
        [HttpPost]
        public async Task<IActionResult> Create(Startup startup)
        {
            startup.UserId = CurrentUserId;  //Привязка к текущему пользователю
            startup.PlatformStartupId = await _startupService.GetPlatformId(startup.NamePlatform);
            if (!ModelState.IsValid)
            {
                await FillListPlatforms();
                return View("CreateStartup", startup);
            }
            await _startupService.Add(startup);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(int id)
        {
            
            var startup = await _startupService.GetAssetById(id);
            if (startup == null || startup.UserId != CurrentUserId)    //Проверка на платформу
                return NotFound();
            return View("DeleteStartup", startup);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var startup = await _startupService.GetAssetById(id);
            if (startup == null || startup.UserId != CurrentUserId)    //Проверка на акции текущего пользователя
                return NotFound();
            await _startupService.Delete(id);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> GetChartT()
        {
            var data = await _startupService.GetChartTicker(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetChartCountComp()
        {
            var data = await _startupService.GetChartCount(CurrentUserId);
            return Json(data);
        }
        /*public async Task<IActionResult> FixSums()
        {
            await _stocksService.FixOldStocks();
            return RedirectToAction("IndexStocks");
        }*/
    }
}
