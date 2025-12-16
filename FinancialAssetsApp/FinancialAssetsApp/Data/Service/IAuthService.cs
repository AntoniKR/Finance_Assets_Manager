using FinancialAssetsApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace FinancialAssetsApp.Data.Service
{
    public interface IAuthService
    {
        Task<User> GetUserByName(string username);  //Получение имени юзера
        Task<bool> ValidateUser(string username, string password);  //Соответствие пароля и имени юзера
        Task<User> RegisterUser(string username, string password);  // Регистрация 
        Task<bool> ChangePassword(string username, string newPassword); // Смена пароля
        Task<bool> UserExists(string username); // Проверка на сущ. юзера
    }
}
