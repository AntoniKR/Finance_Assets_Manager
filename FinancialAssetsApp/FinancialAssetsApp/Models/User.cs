namespace FinancialAssetsApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Hashed password
        public ICollection<Stock> Stocks { get; set; } = new List<Stock>(); // Navigation: RUB stocks
        public ICollection<Crypto> Cryptos { get; set; } = new List<Crypto>(); // Navigation: crypto assets
        public ICollection<Metal> Metals { get; set; } = new List<Metal>(); // Navigation: metals
        public ICollection<StockUSD> StocksUSD { get; set; } = new List<StockUSD>(); // Navigation: USD stocks
        public ICollection<Currency> Currencies { get; set; } = new List<Currency>(); // Navigation: currencies
        public ICollection<Startup> Startups { get; set; } = new List<Startup>(); // Navigation: startups
        public ICollection<PlatformStartup> PlatformStartups { get; set; } = new List<PlatformStartup>(); // Navigation: platforms
    }
}
