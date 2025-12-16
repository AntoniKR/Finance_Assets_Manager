using BCrypt.Net;
using FinancialAssetsApp.Data;
using FinancialAssetsApp.Data.Service;
using FinancialAssetsApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace FinancialAssetsApp.Controllers
{
    public class AccountController : Controller //Авторизация пользователя
    {
        private readonly IAuthService _authService;
        public AccountController(IAuthService authService)  //Доступ к БД через конструктор
        {
            _authService = authService;
        }
        [HttpGet]
        public IActionResult Login()
        {
            ViewData["NoLayout"] = true;    //Убираем плашку layout
            return View();
        }
        //Возвращает страницу логина

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)    //Проверка для входа
        {
            if (!ModelState.IsValid) return View(model);

            if (await _authService.ValidateUser(model.Username, model.Password))
            {
                var user = await _authService.GetUserByName(model.Username);
                HttpContext.Session.SetString("User", user.Username);
                HttpContext.Session.SetInt32("UserId", user.Id);

                return RedirectToAction("Index", "Home");
            }

            //если не совпадает 
            ModelState.AddModelError("", "Неверный логин или пароль");
            return View(model);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)  // Добавление нового пользователя
        {
            if (!ModelState.IsValid) return View(model);

            if(await _authService.UserExists(model.Username))
            {   //Проверка на существующего пользователя
                ModelState.AddModelError("", "Такой пользователь существует");
                return View(model);
            }

            var user = await _authService.RegisterUser(model.Username, model.Password);

            HttpContext.Session.SetString("User", user.Username);
            HttpContext.Session.SetInt32("UserId", user.Id);

            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout()   //Выход из сессии
        {
            HttpContext.Session.Remove("User");
            HttpContext.Session.Remove("UserId");

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult ForgotPassword()   //Замена пароля
        {
            return View();
        }
        //Возвращает страницу логина

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string username, string newPassword)    //Изменение пароля
        {
            if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError("", "Введите логин и новый пароль");
                return View();
            }

            var newPassCompl = await _authService.ChangePassword(username, newPassword);
            if(!newPassCompl)
            {
                ModelState.AddModelError("", "Пользователь не найден");
                return View();
            }

            ViewBag.Message = "Пароль изменен!";
            return RedirectToAction("IndexStocks", "Stocks");
        }
    }
}
