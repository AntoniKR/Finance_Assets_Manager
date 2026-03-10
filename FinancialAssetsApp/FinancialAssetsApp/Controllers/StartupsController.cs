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
        private readonly IAssetData _assetdata; // For fetching various exchange rates
        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
        public StartupsController(IStartupService startupService, IAssetData assetdata)
        {
            _startupService = startupService;
            _assetdata = assetdata;
        }
        public async Task<IActionResult> Index()    // List all startups
        {
            var startup = await _startupService.GetAssetsByID(CurrentUserId);  // Fetch all records from DB
            return View("IndexStartup", startup);
        }
        private async Task FillListPlatforms()    // Populate dropdown with user's added platforms
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
        public async Task<IActionResult> Create()   // Show the add startup page
        {
            await FillListPlatforms();
            return View("CreateStartup");
        }
        [HttpPost]
        public async Task<IActionResult> Create(Startup startup)
        {
            startup.UserId = CurrentUserId;  // Bind asset to the current user
            startup.PlatformStartupId = await _startupService.GetPlatformId(startup.NamePlatform);
            if (!ModelState.IsValid)
            {
                await FillListPlatforms();
                return View("CreateStartup", startup);
            }
            await _startupService.Add(startup);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(int id) // Delete startup
        {
            var startup = await _startupService.GetAssetById(id);
            if (startup == null || startup.UserId != CurrentUserId)    // Verify asset belongs to current user
                return NotFound();
            return View("DeleteStartup", startup);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var startup = await _startupService.GetAssetById(id);
            if (startup == null || startup.UserId != CurrentUserId)    // Verify asset belongs to current user
                return NotFound();
            await _startupService.Delete(id);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> GetChartT()    // Chart for startups by invested sum
        {
            var data = await _startupService.GetChartTicker(CurrentUserId);
            return Json(data);
        }
        public async Task<IActionResult> GetChartCountComp()    // Chart for startups by number of shares
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
