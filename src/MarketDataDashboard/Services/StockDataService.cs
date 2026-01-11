using MarketDataDashboard.Models;
using MarketDataDashboard.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketDataDashboard.Services
{
    public class StockDataService
    {
        private readonly IStockService _api;  // Changed from StockService to IStockService
        private readonly StockContext _db;

        public StockDataService(IStockService api, StockContext db)  // Changed parameter type
        {
            _api = api;
            _db = db;
        }

        public async Task UpdateFromApiAsync(string symbol)
        {
            var apiPoints = await _api.GetStockDataAsync(symbol);
            if (!apiPoints.Any())
                return;

            var stock = await _db.Stocks
                .FirstOrDefaultAsync(s => s.Symbol == symbol);

            if (stock == null)
            {
                stock = new Stock
                {
                    Symbol = symbol,
                    Name = $"{symbol} (Alpha Vantage)"
                };
                _db.Stocks.Add(stock);
                await _db.SaveChangesAsync();
            }

            var existingDates = await _db.PricePoints
                .Where(p => p.StockId == stock.Id)
                .Select(p => p.Date)
                .ToListAsync();

            var newPoints = apiPoints
                .Where(p => !existingDates.Contains(p.Date))
                .Select(p => new PricePoint
                {
                    Date = p.Date,
                    Open = p.Open,
                    High = p.High,
                    Low = p.Low,
                    Close = p.Close,
                    Volume = p.Volume,
                    StockId = stock.Id
                })
                .ToList();

            if (newPoints.Count > 0)
            {
                _db.PricePoints.AddRange(newPoints);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<PricePoint>> GetFromDatabaseAsync(string symbol)
        {
            symbol = symbol.Trim().ToUpper();
            return await _db.PricePoints
                .Where(p => p.Stock != null && p.Stock.Symbol.ToUpper() == symbol)
                .OrderBy(p => p.Date)
                .ToListAsync();
        }
    }
}
