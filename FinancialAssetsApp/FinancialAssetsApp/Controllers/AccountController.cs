using BCrypt.Net;
using FinancialAssetsApp.Data;
using FinancialAssetsApp.Data.Service;
using FinancialAssetsApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace FinancialAssetsApp.Controllers
{
    public class AccountController : Controller // User Authorization
    {
        private readonly IAuthService _authService;
        public AccountController(IAuthService authService)  // Constructor for DB
        {
            _authService = authService;
        }
        [HttpGet]
        public IActionResult Login()
        {
            ViewData["NoLayout"] = true;    // Remove Layout
            return View();
        }
        // Return Login page

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)    // Validate user credentials on login
        {
            if (!ModelState.IsValid) return View(model);

            if (await _authService.ValidateUser(model.Username, model.Password))
            {
                var user = await _authService.GetUserByName(model.Username);
                HttpContext.Session.SetString("User", user.Username);
                HttpContext.Session.SetInt32("UserId", user.Id);

                return RedirectToAction("Index", "Home");
            }

            // Credentials do not match
            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)  // Add new user
        {
            if (!ModelState.IsValid) return View(model);
            if (await _authService.UserExists(model.Username))
            {   // Check if user already exists
                ModelState.AddModelError("", "This username is already taken");
                return View(model);
            }
            var user = await _authService.RegisterUser(model.Username, model.Password);
            HttpContext.Session.SetString("User", user.Username);
            HttpContext.Session.SetInt32("UserId", user.Id);
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout()   // End user session
        {
            HttpContext.Session.Remove("User");
            HttpContext.Session.Remove("UserId");
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public IActionResult ForgotPassword()   // Password reset page
        {
            return View();
        }
        // Returns the login page
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string username, string newPassword)    // Change user password
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError("", "Please enter your username and new password");
                return View();
            }
            var newPassCompl = await _authService.ChangePassword(username, newPassword);
            if (!newPassCompl)
            {
                ModelState.AddModelError("", "User not found");
                return View();
            }
            ViewBag.Message = "Password changed successfully!";
            return RedirectToAction("IndexStocks", "Stocks");
        }
    }
}
