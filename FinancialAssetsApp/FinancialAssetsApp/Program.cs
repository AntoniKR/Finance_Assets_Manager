using FinancialAssetsApp.Data;
using FinancialAssetsApp.Data.Service;
using FinancialAssetsApp.Models;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FinancialAssetsApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);  // Encoding suppport (for API metals)
            builder.Services.AddDbContext<FinanceDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));  // Connect to PostgreSQL

            // Connect MVC
            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpClient<IAssetData, AssetData>();
            builder.Services.AddScoped<IStocksService, StocksService>();
            builder.Services.AddScoped<IStocksUSDService, StocksUSDservice>();
            builder.Services.AddScoped<IAuthService, AuthService>();            
            builder.Services.AddScoped<ICryptosService, CryptosService>();
            builder.Services.AddScoped<HomeService>();
            builder.Services.AddScoped<IMetalsService, MetalsService>();
            builder.Services.AddScoped<ICurrenciesService, CurrenciesService>();
            builder.Services.AddScoped<IPlatformStartupService, PlatformStartupsService>();
            builder.Services.AddScoped<IStartupService, StartupsService>();
            builder.Services.AddScoped<IRealEstateService, RealEstateService>();
            builder.Services.AddScoped<ITransportService, TransportService>();

            builder.Services.AddRazorPages()
                .AddMvcOptions(options =>
                {
                    options.MaxModelValidationErrors = 50;
                    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
                        _ => "Enter your data!");
                });

            //builder.Services.AddDistributedMemoryCache();
            builder.Services.AddMemoryCache();
            builder.Services.AddSession(options =>  // If the session was inactive for 30 minutes, then exit
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            
                // Configure the HTTP request pipeline.
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

            
            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI();
            if (app.Environment.IsDevelopment())
            {
                
            }


            app.UseStaticFiles();
            app.UseSession();

            /*app.Use(async (context, next) =>    //Autologin admin, comment if we want see Login page
            {
                // If the session is not yet established
                if (!context.Session.Keys.Contains("User"))
                {
                    var authService = context.RequestServices.GetRequiredService<IAuthService>();
                    string adminUsername = "admin";
                    string adminPassword = "123";

                    if (await authService.ValidateUser(adminUsername, adminPassword))
                    {
                        var user = await authService.GetUserByName(adminUsername);
                        context.Session.SetString("User", user.Username);
                        context.Session.SetInt32("UserId", user.Id);
                    }
                }

                await next.Invoke();
            });*/

            app.UseRouting();
            app.UseAuthorization(); // Authorization
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");    // Uncomment and the page will load under the admin
                //pattern: "{controller=Home}/{action=Index}/{id?}");         // Comment
            app.Run();
        }
    }
}
