using Microsoft.AspNetCore.Mvc.RazorPages;
using MarketDataDashboard.Services;
using MarketDataDashboard.Models;
using Microsoft.AspNetCore.Mvc;
using MarketDataDashboard.Helpers;

namespace MarketDataDashboard.Pages
{

  public class IndexModel : PageModel
  {
    private readonly StockDataService _dataService;


    private readonly PredictionService _predictionService;

    public IndexModel(StockDataService dataService, PredictionService predictionService)
    {
      _dataService = dataService;
      _predictionService = predictionService;
      ActiveSymbol = "AAPL"; // default active tab
    }


    // Store multiple stock's data
    public Dictionary<string, List<PricePoint>> StocksData { get; set;} = new();

    //  Moving Averages
    public Dictionary<string, List<decimal?>> StocksSMA10 { get; set; } = new();
    public Dictionary<string, List<decimal?>> StocksSMA20 { get; set; } = new();
    public Dictionary<string, List<decimal?>> StocksSMA50 { get; set; } = new();
    public Dictionary<string, List<decimal?>> StocksSMA100 { get; set; } = new();

    // Exponential Moving Averages
    public Dictionary<string, List<decimal?>> StocksEMA10 { get; set; } = new();
    public Dictionary<string, List<decimal?>> StocksEMA20 { get; set; } = new();
    public Dictionary<string, List<decimal?>> StocksEMA50 { get; set; } = new();

    // RSI (Relative Strength Index)
    public Dictionary<string, List<decimal?>> StocksRSI14 { get; set; } = new();
    public Dictionary<string, List<decimal?>> StocksRSI7 { get; set; } = new();

    // Bollinger Bands (upper and lower)
    public Dictionary<string, List<decimal?>> StocksBBUpper { get; set; } = new();
    public Dictionary<string, List<decimal?>> StocksBBLower { get; set; } = new();

    // MACD (trend indicator)
    public Dictionary<string, List<decimal?>> StocksMACD { get; set; } = new();
    public Dictionary<string, List<decimal?>> StocksMACDSignal { get; set; } = new();

    public Dictionary<string, List<decimal?>> StocksPrediction { get; set; } = new();

    // List of stock symbols we want to display
    public List<string> StockSymbols { get; } = new () { "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA" };
    public string ActiveSymbol { get; set; }


    public async Task OnGetAsync(string symbol)
    {
      if (!string.IsNullOrEmpty(symbol))
          ActiveSymbol = symbol;
      foreach (var s in StockSymbols)
      {
        var data = await _dataService.GetFromDatabaseAsync(s);
        StocksData[s] = data;

        // SMA
        StocksSMA10[s] = TechnicalIndicators.SMA(data, 10);
        StocksSMA20[s] = TechnicalIndicators.SMA(data, 20);
        StocksSMA50[s] = TechnicalIndicators.SMA(data, 50);
        StocksSMA100[s] = TechnicalIndicators.SMA(data, 100);

        // EMA
        StocksEMA10[s] = TechnicalIndicators.EMA(data, 10);
        StocksEMA20[s] = TechnicalIndicators.EMA(data, 20);
        StocksEMA50[s] = TechnicalIndicators.EMA(data, 50);

        // RSI
        StocksRSI14[s] = TechnicalIndicators.RSI(data, 14);
        StocksRSI7[s] = TechnicalIndicators.RSI(data, 7);

        // Bollinger Bands
        var (upper, lower) = TechnicalIndicators.BollingerBands(data, 20, 2);
        StocksBBUpper[s] = upper;
        StocksBBLower[s] = lower;

        // MACD
        var (macd, signal) = TechnicalIndicators.MACD(data);
        StocksMACD[s] = macd;
        StocksMACDSignal[s] = signal;

      }
    }

    // Update data for one stock
    public async Task<IActionResult> OnPostUpdateAsync(string symbol)
    {
        await _dataService.UpdateFromApiAsync(symbol);
        return RedirectToPage(new { symbol });
    }

    // Download CSV for one stock
    public async Task<IActionResult> OnGetDownloadAsync(string symbol)
    {
      var data = await _dataService.GetFromDatabaseAsync(symbol);
      var csv = CsvExporter.Export(data);

      return File(
          System.Text.Encoding.UTF8.GetBytes(csv),
          "text/csv",
          $"{symbol}-lastest.csv"
      );
    }

    public Task<IActionResult> OnPostPredictAsync(string symbol)
    {
        // Start prediction in background
      _predictionService.StartPrediction(symbol);


      // Immediately initialize empty list if not exists
      if (!StocksPrediction.ContainsKey(symbol))
          StocksPrediction[symbol] = new List<decimal?>();

      // Immediately return to page, do not wait
      return Task.FromResult<IActionResult>(RedirectToPage(new { symbol }));
    }

    public IActionResult OnGetPredictionResult(string symbol)
    {
        if (_predictionService.TryGetPrediction(symbol, out var result)
            && result.PredictedValues.Count > 0)
        {
            // Convert DateTime to string format for JavaScript
            return new JsonResult(new
            {
                predictedValues = result.PredictedValues,
                futureDates = result.Dates.Select(d => d.ToString("yyyy-MM-dd")).ToList()
            });
        }
        return new JsonResult(null);
    }
  }
}
