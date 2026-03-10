using FinancialAssetsApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace FinancialAssetsApp.Data.Service
{
    public interface IAuthService
    {
        Task<User> GetUserByName(string username);  // Get user by username
        Task<bool> ValidateUser(string username, string password);  // Validate username and password match
        Task<User> RegisterUser(string username, string password);  // Register new user
        Task<bool> ChangePassword(string username, string newPassword); // Change user password
        Task<bool> UserExists(string username); // Check if user already exists
    }
}
