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
            var data = await _homeService.GetAssetsSumInvested(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetPurchaseAssets()
        {
            var data = await _homeService.GetPurchaseTotal(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetCurrentAssets()
        {
            var data = await _homeService.GetCurrentAss(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetChangeSum()
        {
            var currSum = await _homeService.GetCurrentAss(CurrentUserId);
            var purchSum = await _homeService.GetPurchaseTotal(CurrentUserId);
            var changeSum = currSum - purchSum;
            return Json(changeSum);
        }
        public async Task<IActionResult> GetPercentageChangeSum()
        {
            var currSum = await _homeService.GetCurrentAss(CurrentUserId);
            var purchSum = await _homeService.GetPurchaseTotal(CurrentUserId);
            var changePercentSum = (((currSum - purchSum) / purchSum) * 100);
            return Json(changePercentSum);
        }




        public async Task<IActionResult> GetETrChart()   //получение общей суммы активов
        {
            var data = await _homeService.GetEstateTransSumm(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetRateContr() // Rate USD
        {
            var data = await _homeService.GetRate();
            return Json(data);
        }



    }
}
