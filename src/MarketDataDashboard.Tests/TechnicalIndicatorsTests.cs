using MarketDataDashboard.Helpers;
using MarketDataDashboard.Models;

namespace MarketDataDashboard.Tests;

public class TechnicalIndicatorsTests
{
    #region Test Data Helpers

    private static List<PricePoint> CreatePricePoints(params decimal[] closes)
    {
        return closes.Select((close, i) => new PricePoint
        {
            Date = DateTime.Today.AddDays(i),
            Open = close,
            High = close + 1,
            Low = close - 1,
            Close = close,
            Volume = 1000
        }).ToList();
    }

    #endregion

    #region SMA Tests

    [Fact]
    public void SMA_WithValidData_ReturnsCorrectAverage()
    {
        // Arrange
        var data = CreatePricePoints(10, 20, 30, 40, 50);

        // Act
        var result = TechnicalIndicators.SMA(data, 3);

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Null(result[0]); // Not enough data
        Assert.Null(result[1]); // Not enough data
        Assert.Equal(20m, result[2]); // (10+20+30)/3
        Assert.Equal(30m, result[3]); // (20+30+40)/3
        Assert.Equal(40m, result[4]); // (30+40+50)/3
    }

    [Fact]
    public void SMA_WithPeriodLargerThanData_ReturnsAllNulls()
    {
        var data = CreatePricePoints(10, 20, 30);

        var result = TechnicalIndicators.SMA(data, 10);

        Assert.All(result, value => Assert.Null(value));
    }

    [Fact]
    public void SMA_WithEmptyData_ReturnsEmptyList()
    {
        var data = new List<PricePoint>();

        var result = TechnicalIndicators.SMA(data, 5);

        Assert.Empty(result);
    }

    [Fact]
    public void SMA_WithPeriodOne_ReturnsOriginalValues()
    {
        var data = CreatePricePoints(10, 20, 30);

        var result = TechnicalIndicators.SMA(data, 1);

        Assert.Equal(10m, result[0]);
        Assert.Equal(20m, result[1]);
        Assert.Equal(30m, result[2]);
    }

    #endregion

    #region EMA Tests

    [Fact]
    public void EMA_WithValidData_ReturnsValues()
    {
        var data = CreatePricePoints(10, 20, 30, 40, 50);

        var result = TechnicalIndicators.EMA(data, 3);

        Assert.Equal(5, result.Count);
        Assert.Null(result[0]);
        Assert.Null(result[1]);
        Assert.NotNull(result[2]); // First EMA = SMA
        Assert.NotNull(result[3]);
        Assert.NotNull(result[4]);
    }

    [Fact]
    public void EMA_FirstValue_EqualsSMA()
    {
        var data = CreatePricePoints(10, 20, 30, 40, 50);

        var ema = TechnicalIndicators.EMA(data, 3);
        var sma = TechnicalIndicators.SMA(data, 3);

        // First EMA value should equal first SMA value
        Assert.Equal(sma[2], ema[2]);
    }

    [Fact]
    public void EMA_RespondsToRecentPrices()
    {
        // Test that EMA produces valid values for rising prices
        var data = CreatePricePoints(10, 20, 30, 40, 50);

        var ema = TechnicalIndicators.EMA(data, 3);

        // EMA should have values after the initial period
        Assert.NotNull(ema[2]);
        Assert.NotNull(ema[3]);
        Assert.NotNull(ema[4]);

        // EMA should trend upward with rising prices
        Assert.True(ema[4] > ema[3], $"EMA[4]={ema[4]} should be > EMA[3]={ema[3]}");
        Assert.True(ema[3] > ema[2], $"EMA[3]={ema[3]} should be > EMA[2]={ema[2]}");
    }

    #endregion

    #region RSI Tests

    [Fact]
    public void RSI_WithValidData_ReturnsBoundedValues()
    {
        var data = CreatePricePoints(44, 44.5m, 43.5m, 44.5m, 45, 45.5m, 46, 45, 44, 43, 42, 43, 44, 45, 46);

        var result = TechnicalIndicators.RSI(data, 14);

        // RSI should be between 0 and 100
        foreach (var value in result.Where(v => v.HasValue))
        {
            Assert.InRange(value!.Value, 0, 100);
        }
    }

    [Fact]
    public void RSI_AllGains_ReturnsHighValue()
    {
        // Continuously rising prices
        var data = CreatePricePoints(10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24);

        var result = TechnicalIndicators.RSI(data, 14);

        // RSI should be close to 100 for all gains
        var lastRsi = result.Last();
        Assert.NotNull(lastRsi);
        Assert.True(lastRsi > 70, $"RSI was {lastRsi}, expected > 70");
    }

    [Fact]
    public void RSI_AllLosses_ReturnsLowValue()
    {
        // Continuously falling prices
        var data = CreatePricePoints(24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10);

        var result = TechnicalIndicators.RSI(data, 14);

        var lastRsi = result.Last();
        Assert.NotNull(lastRsi);
        Assert.True(lastRsi < 30, $"RSI was {lastRsi}, expected < 30");
    }

    #endregion

    #region Bollinger Bands Tests

    [Fact]
    public void BollingerBands_ReturnsUpperAndLower()
    {
        var data = CreatePricePoints(20, 21, 22, 21, 20, 19, 20, 21, 22, 23, 22, 21, 20, 19, 18, 19, 20, 21, 22, 23);

        var (upper, lower) = TechnicalIndicators.BollingerBands(data, 20, 2);

        Assert.Equal(data.Count, upper.Count);
        Assert.Equal(data.Count, lower.Count);
    }

    [Fact]
    public void BollingerBands_UpperAboveLower()
    {
        var data = CreatePricePoints(20, 21, 22, 21, 20, 19, 20, 21, 22, 23, 22, 21, 20, 19, 18, 19, 20, 21, 22, 23);

        var (upper, lower) = TechnicalIndicators.BollingerBands(data, 20, 2);

        for (int i = 0; i < upper.Count; i++)
        {
            if (upper[i].HasValue && lower[i].HasValue)
            {
                Assert.True(upper[i] > lower[i], $"At index {i}: upper {upper[i]} should be > lower {lower[i]}");
            }
        }
    }

    [Fact]
    public void BollingerBands_ConstantPrice_BandsConverge()
    {
        // All same price = zero volatility = bands equal SMA
        var data = CreatePricePoints(50, 50, 50, 50, 50);

        var (upper, lower) = TechnicalIndicators.BollingerBands(data, 5, 2);

        // With no volatility, upper and lower should equal the SMA
        Assert.Equal(upper[4], lower[4]);
        Assert.Equal(50m, upper[4]);
    }

    #endregion

    #region MACD Tests

    [Fact]
    public void MACD_ReturnsCorrectLength()
    {
        var data = CreatePricePoints(Enumerable.Range(1, 30).Select(i => (decimal)i).ToArray());

        var (macd, signal) = TechnicalIndicators.MACD(data);

        Assert.Equal(data.Count, macd.Count);
        Assert.Equal(data.Count, signal.Count);
    }

    [Fact]
    public void MACD_FirstValuesAreNull()
    {
        var data = CreatePricePoints(Enumerable.Range(1, 30).Select(i => (decimal)i).ToArray());

        var (macd, signal) = TechnicalIndicators.MACD(data);

        // MACD needs 26 periods for EMA26
        for (int i = 0; i < 25; i++)
        {
            Assert.Null(macd[i]);
        }
    }

    #endregion
}
