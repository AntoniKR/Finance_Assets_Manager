using Microsoft.EntityFrameworkCore;
using FinancialAssetsApp.Models;


namespace FinancialAssetsApp.Data
{
    public class FinanceDbContext : DbContext
    {
        public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options) { } // Конструктор по умолчанию

        public DbSet<Stock> Stocks { get; set; }    // Для взаимодействия с БД, хранящая акции
        public DbSet<User> Users { get; set; } // БД, хранящая юзеров
        public DbSet<Crypto> Cryptos { get; set; } // БД, хранящая крипту
        public DbSet<Metal> Metals { get; set; } // БД, хранящая металлы
        public DbSet<StockUSD> StocksUSD { get; set; } // БД, хранящая акции USD
        public DbSet<Currency> Currencies { get; set; } // БД, хранящая валюты
        public DbSet<Startup> Startups { get; set; } // БД, хранящая стартапы
        public DbSet<PlatformStartup> PlatformStartups { get; set; } // БД, хранящая платформы стартапов
        public DbSet<RealEstate> RealEstates { get; set; } // БД, хранящая недвижимость
        public DbSet<Transport> Transports { get; set; } // БД, хранящая транспорт



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach(var diffKey in modelBuilder.Model   //При удалении пользователя удаление всех активов
                .GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys())
                .Where(fk => fk.PrincipalEntityType.ClrType == typeof(User)))
            {
                diffKey.DeleteBehavior = DeleteBehavior.Cascade;
            }

            modelBuilder.Entity<Startup>()      // Каскадное удаление стартапов, если удалить платформу
                .HasOne(s => s.PlatformStartup)
                .WithMany()
                .HasForeignKey(s => s.PlatformStartupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
