using Microsoft.EntityFrameworkCore;
using Moq;
using MarketDataDashboard.Data;
using MarketDataDashboard.Models;
using MarketDataDashboard.Services;

namespace MarketDataDashboard.Tests;

public class StockDataServiceTests : IDisposable
{
    private readonly StockContext _context;
    private readonly Mock<IStockService> _mockStockService;  // Mock interface, not class
    private readonly StockDataService _service;

    public StockDataServiceTests()
    {
        var options = new DbContextOptionsBuilder<StockContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StockContext(options);
        _mockStockService = new Mock<IStockService>();  // Mock the interface
        _service = new StockDataService(_mockStockService.Object, _context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetFromDatabaseAsync Tests

    [Fact]
    public async Task GetFromDatabaseAsync_WithExistingData_ReturnsOrderedByDate()
    {
        var stock = new Stock { Symbol = "AAPL", Name = "Apple" };
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        _context.PricePoints.AddRange(
            new PricePoint { Date = new DateTime(2024, 1, 3), Close = 103, StockId = stock.Id },
            new PricePoint { Date = new DateTime(2024, 1, 1), Close = 100, StockId = stock.Id },
            new PricePoint { Date = new DateTime(2024, 1, 2), Close = 102, StockId = stock.Id }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetFromDatabaseAsync("AAPL");

        Assert.Equal(3, result.Count);
        Assert.Equal(new DateTime(2024, 1, 1), result[0].Date);
        Assert.Equal(new DateTime(2024, 1, 2), result[1].Date);
        Assert.Equal(new DateTime(2024, 1, 3), result[2].Date);
    }

    [Fact]
    public async Task GetFromDatabaseAsync_WithNoData_ReturnsEmptyList()
    {
        var result = await _service.GetFromDatabaseAsync("AAPL");
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFromDatabaseAsync_IsCaseInsensitive()
    {
        var stock = new Stock { Symbol = "AAPL", Name = "Apple" };
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        _context.PricePoints.Add(new PricePoint { Date = DateTime.Today, Close = 100, StockId = stock.Id });
        await _context.SaveChangesAsync();

        var result1 = await _service.GetFromDatabaseAsync("aapl");
        var result2 = await _service.GetFromDatabaseAsync("AAPL");
        var result3 = await _service.GetFromDatabaseAsync("AaPl");

        Assert.Single(result1);
        Assert.Single(result2);
        Assert.Single(result3);
    }

    [Fact]
    public async Task GetFromDatabaseAsync_TrimsWhitespace()
    {
        var stock = new Stock { Symbol = "AAPL", Name = "Apple" };
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        _context.PricePoints.Add(new PricePoint { Date = DateTime.Today, Close = 100, StockId = stock.Id });
        await _context.SaveChangesAsync();

        var result = await _service.GetFromDatabaseAsync("  AAPL  ");
        Assert.Single(result);
    }

    #endregion

    #region UpdateFromApiAsync Tests

    [Fact]
    public async Task UpdateFromApiAsync_WithNewStock_CreatesStock()
    {
        _mockStockService
            .Setup(s => s.GetStockDataAsync("AAPL"))
            .ReturnsAsync(new List<PricePoint>
            {
                new() { Date = DateTime.Today, Open = 100, High = 105, Low = 99, Close = 103, Volume = 1000 }
            });

        await _service.UpdateFromApiAsync("AAPL");

        var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == "AAPL");
        Assert.NotNull(stock);
        Assert.Equal("AAPL (Alpha Vantage)", stock.Name);
    }

    [Fact]
    public async Task UpdateFromApiAsync_OnlyAddsNewDates()
    {
        var stock = new Stock { Symbol = "AAPL", Name = "Apple" };
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        _context.PricePoints.Add(new PricePoint
        {
            Date = new DateTime(2024, 1, 1),
            Close = 100,
            StockId = stock.Id
        });
        await _context.SaveChangesAsync();

        _mockStockService
            .Setup(s => s.GetStockDataAsync("AAPL"))
            .ReturnsAsync(new List<PricePoint>
            {
                new() { Date = new DateTime(2024, 1, 1), Close = 100 },
                new() { Date = new DateTime(2024, 1, 2), Close = 102 }
            });

        await _service.UpdateFromApiAsync("AAPL");

        var points = await _context.PricePoints.Where(p => p.StockId == stock.Id).ToListAsync();
        Assert.Equal(2, points.Count);
    }

    [Fact]
    public async Task UpdateFromApiAsync_WithEmptyApiResponse_DoesNothing()
    {
        _mockStockService
            .Setup(s => s.GetStockDataAsync("AAPL"))
            .ReturnsAsync(new List<PricePoint>());

        await _service.UpdateFromApiAsync("AAPL");

        Assert.Empty(await _context.Stocks.ToListAsync());
        Assert.Empty(await _context.PricePoints.ToListAsync());
    }

    #endregion
}
