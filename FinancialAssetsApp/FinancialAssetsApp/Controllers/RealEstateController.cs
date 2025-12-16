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
    public class RealEstateController : Controller
    {
        private readonly IRealEstateService _realestateService;
        private readonly IAssetData _assetdata; // Для парсинга различных курсов
        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
        public RealEstateController(IRealEstateService realestateService, IAssetData assetdata)
        {
            _realestateService = realestateService;
            _assetdata = assetdata;
        }

        public async Task<IActionResult> IndexEstate()    // Список всей недвижимости
        {
            var realestates = await _realestateService.GetAssetsByID(CurrentUserId);  // Перечисление всех данных из БД
            return View("IndexEstate", realestates);
        }
        private void FillListEstate()    // Метод для списка типов недвижимости 
        {
            ViewBag.REType = new List<SelectListItem>        // Создание списка для выбора металла
            {
                new SelectListItem {Value = "Квартира", Text = "Квартира"},
                new SelectListItem {Value = "Дачный участок", Text = "Дачный участок"},
                new SelectListItem {Value = "Земельный участок", Text = "Земельный участок"},
                new SelectListItem {Value = "Офисная недвижимость", Text = "Офисная недвижимость"},
                new SelectListItem {Value = "Складская недвижимость", Text = "Складская недвижимость"},
                new SelectListItem {Value = "Гостиничная недвижимость", Text = "Гостиничная недвижимость"},
                new SelectListItem {Value = "Торговая недвижимость", Text = "Торговая недвижимость"},
            };
        }
        public IActionResult Create()   // Страница добавления недвижимости
        {

            FillListEstate();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(RealEstate realEstate)
        {
            realEstate.UserId = CurrentUserId;  //Привязка к текущему пользователю
            
            if (!ModelState.IsValid)
            {
                FillListEstate();
                return View(realEstate);
            }
            await _realestateService.Add(realEstate);
            return RedirectToAction("IndexEstate");
        }
        public async Task<IActionResult> Delete(int id)
        {
            
            var realEstate = await _realestateService.GetAssetById(id);
            if (realEstate == null || realEstate.UserId != CurrentUserId)    //Проверка на акции текущего пользователя
                return NotFound();
            return View("DeleteEstate", realEstate);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var realEstate = await _realestateService.GetAssetById(id);
            if (realEstate == null || realEstate.UserId != CurrentUserId)    //Проверка на акции текущего пользователя
                return NotFound();
            await _realestateService.Delete(id);
            return RedirectToAction("IndexEstate");
        }
        public async Task<IActionResult> GetChartC()
        {
            var data = await _realestateService.GetChartCities(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetChartT()
        {
            var data = await _realestateService.GetChartType(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> ListCities(string term)   //Получение списка городов
        {
            var dataCities = await _assetdata.GetCitiesList(term);
            return Json(dataCities);
        }


        /*public async Task<IActionResult> FixSums()
        {
            await _stocksService.FixOldStocks();
            return RedirectToAction("IndexStocks");
        }*/
    }
}
