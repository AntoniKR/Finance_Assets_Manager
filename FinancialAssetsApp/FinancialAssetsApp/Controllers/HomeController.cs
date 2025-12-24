using System.Diagnostics;
using FinancialAssetsApp.Data.Service;
using FinancialAssetsApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinancialAssetsApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HomeService _homeService;
        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

        public HomeController(ILogger<HomeController> logger, HomeService homeService)
        {
            _logger = logger;
            _homeService = homeService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<IActionResult> GetAssetsChart()   //получение общей суммы активов
        {
            var data = await _homeService.GetAssetsSumm(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetETrChart()   //получение общей суммы активов
        {
            var data = await _homeService.GetEstateTransSumm(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetRateContr()
        {
            var data = await _homeService.GetRate();
            return Json(data);
        }
        public async Task<IActionResult> GetCurrCrypto()
        {
            var data = await _homeService.GetCurrentCryptoSUM(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetCurrMetals()
        {
            var data = await _homeService.GetCurrentMetalSUM(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetCurrAssets()
        {
            var data = await _homeService.GetCurrentAss(CurrentUserId);
            return Json(data);
        }

    }
}
