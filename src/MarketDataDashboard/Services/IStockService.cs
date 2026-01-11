using MarketDataDashboard.Models;

namespace MarketDataDashboard.Services
{
    public interface IStockService
    {
        Task<List<PricePoint>> GetStockDataAsync(string symbol);
    }
}
