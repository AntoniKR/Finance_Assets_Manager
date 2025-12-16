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
    public class TransportController : Controller
    {
        private readonly ITransportService _transportService;
        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
        public TransportController(ITransportService transportService)
        {
            _transportService = transportService;
        }

        public async Task<IActionResult> IndexTransport()    // Список всего транспорта
        {
            var transports = await _transportService.GetAssetsByID(CurrentUserId);  // Перечисление всех данных из БД
            return View("IndexTransport", transports);
        }
        private void FillListTransport()    // Метод для списка типов транспорта 
        {
            ViewBag.TransportType = new List<SelectListItem>        // Создание списка для выбора транспорта
            {
                new SelectListItem {Value = "Велосипед", Text = "Велосипед"},
                new SelectListItem {Value = "Мотоцикл", Text = "Мотоцикл"},
                new SelectListItem {Value = "Машина", Text = "Машина"},
                new SelectListItem {Value = "Скутер", Text = "Скутер"},
                new SelectListItem {Value = "Электросамокат", Text = "Электросамокат"}
            };
        }
        public IActionResult Create()   // Страница добавления транспорта
        {

            FillListTransport();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Transport transport)
        {
            transport.UserId = CurrentUserId;  //Привязка к текущему пользователю
            if (transport.YearOfTransport == null)
                transport.YearOfTransport = DateTime.UtcNow.Year;
            if (!ModelState.IsValid)
            {
                FillListTransport();
                return View(transport);
            }
            await _transportService.Add(transport);
            return RedirectToAction("IndexTransport");
        }
        public async Task<IActionResult> Delete(int id)
        {
            
            var transport = await _transportService.GetAssetById(id);
            if (transport == null || transport.UserId != CurrentUserId)    //Проверка на акции текущего пользователя
                return NotFound();
            return View("DeleteTransport", transport);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transport = await _transportService.GetAssetById(id);
            if (transport == null || transport.UserId != CurrentUserId)    //Проверка на акции текущего пользователя
                return NotFound();
            await _transportService.Delete(id);
            return RedirectToAction("IndexTransport");
        }
        public async Task<IActionResult> GetChartTTrans()
        {
            var data = await _transportService.GetChartTypeTrans(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetChartSTrans()
        {
            var data = await _transportService.GetChartSumTrans(CurrentUserId);
            return Json(data);
        }

        /*public async Task<IActionResult> FixSums()
        {
            await _stocksService.FixOldStocks();
            return RedirectToAction("IndexStocks");
        }*/
    }
}
