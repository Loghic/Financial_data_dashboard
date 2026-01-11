using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MarketDataDashboard.Data;
using MarketDataDashboard.Models;
using MarketDataDashboard.Services;
namespace MarketDataDashboard.Tests;

public class PredictionServiceTests
{
    #region Helper Methods Tests (via reflection or make them internal/public)

    [Fact]
    public void GenerateFutureDates_SkipsWeekends()
    {
        // Friday Jan 5, 2024
        var friday = new DateTime(2024, 1, 5);

        var service = CreatePredictionService();

        // Use reflection to test private method, or make it internal with [InternalsVisibleTo]
        var method = typeof(PredictionService).GetMethod("GenerateFutureDates",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var result = (List<DateTime>)method!.Invoke(service, new object[] { friday, 5 })!;

        // Friday -> Mon, Tue, Wed, Thu, Fri (skips Sat/Sun)
        Assert.Equal(new DateTime(2024, 1, 8), result[0]);  // Monday
        Assert.Equal(new DateTime(2024, 1, 9), result[1]);  // Tuesday
        Assert.Equal(new DateTime(2024, 1, 10), result[2]); // Wednesday
        Assert.Equal(new DateTime(2024, 1, 11), result[3]); // Thursday
        Assert.Equal(new DateTime(2024, 1, 12), result[4]); // Friday
    }

    [Fact]
    public void GetNextBusinessDay_FromFriday_ReturnsMonday()
    {
        var friday = new DateTime(2024, 1, 5);

        var service = CreatePredictionService();
        var method = typeof(PredictionService).GetMethod("GetNextBusinessDay",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var result = (DateTime)method!.Invoke(service, new object[] { friday })!;

        Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
        Assert.Equal(new DateTime(2024, 1, 8), result);
    }

    [Fact]
    public void GetNextBusinessDay_FromSaturday_ReturnsMonday()
    {
        var saturday = new DateTime(2024, 1, 6);

        var service = CreatePredictionService();
        var method = typeof(PredictionService).GetMethod("GetNextBusinessDay",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var result = (DateTime)method!.Invoke(service, new object[] { saturday })!;

        Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
    }

    [Fact]
    public void GetNextBusinessDay_FromWednesday_ReturnsThursday()
    {
        var wednesday = new DateTime(2024, 1, 10);

        var service = CreatePredictionService();
        var method = typeof(PredictionService).GetMethod("GetNextBusinessDay",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var result = (DateTime)method!.Invoke(service, new object[] { wednesday })!;

        Assert.Equal(DayOfWeek.Thursday, result.DayOfWeek);
    }

    #endregion

    #region TryGetPrediction Tests

    [Fact]
    public void TryGetPrediction_BeforeCompletion_ReturnsFalse()
    {
        var service = CreatePredictionService();

        var found = service.TryGetPrediction("AAPL", out var result);

        Assert.False(found);
    }

    [Fact]
    public void StartPrediction_ClearsPreviousResult()
    {
        var service = CreatePredictionService();

        // Simulate a completed prediction by accessing the private dictionary
        var field = typeof(PredictionService).GetField("_completed",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dict = (System.Collections.Concurrent.ConcurrentDictionary<string, PredictionResult>)field!.GetValue(service)!;

        dict["AAPL"] = new PredictionResult { Symbol = "AAPL", PredictedValues = new List<decimal> { 100 } };

        // Start new prediction (should clear old one)
        service.StartPrediction("AAPL");

        // Old result should be cleared
        Assert.False(dict.ContainsKey("AAPL") && dict["AAPL"].PredictedValues.Count > 0);
    }

    #endregion

    #region Helpers

    private PredictionService CreatePredictionService()
    {
        var services = new ServiceCollection();

        // Register mock IStockService
        var mockStockService = new Mock<IStockService>();
        services.AddSingleton(mockStockService.Object);

        // Register mock StockContext with in-memory database
        var options = new DbContextOptionsBuilder<StockContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        services.AddScoped(_ => new StockContext(options));

        // Register StockDataService
        services.AddScoped<StockDataService>();

        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        return new PredictionService(scopeFactory);
    }

    #endregion
}
