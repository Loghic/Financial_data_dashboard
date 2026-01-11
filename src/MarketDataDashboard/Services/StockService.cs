using System.Globalization; // For CultureInfo (parsing decimal '.')
using System.Text.Json.Serialization; // For JsonPropertyName
using MarketDataDashboard.Models;

namespace MarketDataDashboard.Services
{
  public class StockService : IStockService
  {
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public StockService(HttpClient http, string apiKey)
    {
      _http = http;
      _apiKey = apiKey;
    }

    // Fetch stock data using Alpha vantage API
    public async Task<List<PricePoint>> GetStockDataAsync(string symbol)
    {
      try
      {
        var url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={_apiKey}";

        var response = await _http.GetFromJsonAsync<AlphaResponse>(url);
        if (response == null || response.TimeSeries == null)
        {
          Console.WriteLine("No data returned. Maybe API limit reached?");
          return new List<PricePoint>();
        }

        // Map API data to PricePoint
        var points = new List<PricePoint>();
        foreach (var kvp in response.TimeSeries)
        {
          #if DEBUG
            Console.WriteLine($"{kvp.Key}: Open={kvp.Value["1. open"]}, Close={kvp.Value["4. close"]}");
          #endif

          var date = DateTime.Parse(kvp.Key);
          var data = kvp.Value;

          points.Add(new PricePoint
              {
                Date = date,
                Open = decimal.Parse(data["1. open"], CultureInfo.InvariantCulture),
                High = decimal.Parse(data["2. high"], CultureInfo.InvariantCulture),
                Low = decimal.Parse(data["3. low"], CultureInfo.InvariantCulture),
                Close = decimal.Parse(data["4. close"], CultureInfo.InvariantCulture),
                Volume = long.Parse(data["5. volume"], CultureInfo.InvariantCulture)
              });
        }

        return points.OrderBy(p => p.Date).ToList();
      }
      catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
      {
        Console.WriteLine($"Rate limit exceed for {symbol}: {ex.Message}");
        return new List<PricePoint>();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error fetching data for {symbol}: {ex.Message}");
        return new List<PricePoint>();
      }
    }
  }

  // Helper classes to deserialize Yahoo Finance response
  public class AlphaResponse
  {
      [JsonPropertyName("Time Series (Daily)")]
      public Dictionary<string, Dictionary<string, string>> TimeSeries { get; set; } = new();
  }
}

