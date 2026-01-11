using MarketDataDashboard.Helpers;
using MarketDataDashboard.Models;

namespace MarketDataDashboard.Tests;

public class CsvExporterTests
{
    [Fact]
    public void Export_WithValidData_ContainsHeader()
    {
        var data = new List<PricePoint>
        {
            new() { Date = new DateTime(2024, 1, 15), Open = 100, High = 105, Low = 99, Close = 103, Volume = 1000000 }
        };

        var result = CsvExporter.Export(data);

        Assert.StartsWith("Date,Open,High,Low,Close,Volume", result);
    }

    [Fact]
    public void Export_WithValidData_FormatsCorrectly()
    {
        var data = new List<PricePoint>
        {
            new() { Date = new DateTime(2024, 1, 15), Open = 100.123m, High = 105.456m, Low = 99.789m, Close = 103.012m, Volume = 1000000 }
        };

        var result = CsvExporter.Export(data);
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(2, lines.Length); // Header + 1 row
        Assert.Contains("2024-01-15", lines[1]);
        Assert.Contains("100.123", lines[1]);
        Assert.Contains("1000000", lines[1]);
    }

    [Fact]
    public void Export_WithEmptyData_ReturnsOnlyHeader()
    {
        var data = new List<PricePoint>();

        var result = CsvExporter.Export(data);

        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(lines);
        Assert.Contains("Date,Open,High,Low,Close,Volume", lines[0]);
    }

    [Fact]
    public void Export_WithMultipleRows_ExportsAll()
    {
        var data = new List<PricePoint>
        {
            new() { Date = new DateTime(2024, 1, 1), Open = 100, High = 105, Low = 99, Close = 103, Volume = 1000 },
            new() { Date = new DateTime(2024, 1, 2), Open = 103, High = 108, Low = 102, Close = 107, Volume = 2000 },
            new() { Date = new DateTime(2024, 1, 3), Open = 107, High = 110, Low = 105, Close = 109, Volume = 3000 }
        };

        var result = CsvExporter.Export(data);
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(4, lines.Length); // Header + 3 rows
    }

    [Fact]
    public void Export_UsesInvariantCulture()
    {
        // Ensures decimal separator is always '.' regardless of system culture
        var data = new List<PricePoint>
        {
            new() { Date = new DateTime(2024, 1, 1), Open = 1234.567m, High = 1234.567m, Low = 1234.567m, Close = 1234.567m, Volume = 1000 }
        };

        var result = CsvExporter.Export(data);

        Assert.Contains("1234.567", result);
        Assert.DoesNotContain("1234,567", result); // Not comma as decimal separator
    }
}
