using FinancialAssetsApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialAssetsApp.Data.Service
{
    public class AuthService: IAuthService
    {
        public readonly FinanceDbContext _authUser;
        public AuthService(FinanceDbContext authUser)
        {
            _authUser = authUser;
        }
        public async Task<User> RegisterUser(string username, string password)  //регистрация юзера и добавление в БД
        {
            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };
            _authUser.Users.Add(user);
            await _authUser.SaveChangesAsync();
            return user;
        }

        public async Task<bool> ChangePassword(string username, string newPassword) // Смена пароля пользователя
        {
            var user = await GetUserByName(username);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _authUser.SaveChangesAsync();
            return true;
        }

        public async Task<User> GetUserByName(string username)  //получение имени пользователя
        {
            return await _authUser.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> UserExists(string username) //Проверка на сущ. юзера
        {
            return await _authUser.Users.AnyAsync(u => u.Username == username); 
        }

        public async Task<bool> ValidateUser(string username, string password)  //Проверка пароля пользователя
        {
            var user = await GetUserByName(username);
            return user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
    }
}
