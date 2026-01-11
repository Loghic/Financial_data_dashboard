using System.Diagnostics;
using System.Text.Json;
using MarketDataDashboard.Models;
using System.Collections.Concurrent;

namespace MarketDataDashboard.Services
{
    public class PredictionService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<string, PredictionResult> _completed = new();

        public PredictionService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void StartPrediction(string symbol)
        {
            // Clear any previous prediction for this symbol
            _completed.TryRemove(symbol, out _);

            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await RunPredictionProcessAsync(symbol);
                    _completed[symbol] = result;
                    Console.WriteLine($"[PredictionService] Completed for {symbol}: {result.PredictedValues.Count} values, {result.Dates.Count} dates");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PredictionService] Error for {symbol}: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
            });
        }

        public bool TryGetPrediction(string symbol, out PredictionResult result) =>
            _completed.TryGetValue(symbol, out result!);

        private async Task<PredictionResult> RunPredictionProcessAsync(string symbol)
        {
            using var scope = _scopeFactory.CreateScope();
            var stockDataService = scope.ServiceProvider.GetRequiredService<StockDataService>();

            var historicalData = await stockDataService.GetFromDatabaseAsync(symbol);

            if (historicalData.Count < 10)
                return new PredictionResult { Symbol = symbol };

            // Get the last date to calculate future dates
            var lastDate = historicalData.Max(p => p.Date);

            var input = new PredictionInput
            {
                Symbol = symbol,
                Dates = historicalData.Select(p => p.Date).ToList(),
                ClosePrices = historicalData.Select(p => p.Close).ToList()
            };

            var jsonInput = JsonSerializer.Serialize(input);
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "python", "predict.py");
            var interpreterPath = Path.Combine(AppContext.BaseDirectory, "python", "venv", "bin", "python");

            var psi = new ProcessStartInfo
            {
                FileName = interpreterPath,
                Arguments = $"\"{scriptPath}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start Python process");

            await process.StandardInput.WriteAsync(jsonInput);
            process.StandardInput.Close();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(error))
            {
                Console.WriteLine($"[PredictionService] Python stderr: {error}");
                if (process.ExitCode != 0)
                    throw new InvalidOperationException($"Python error: {error}");
            }

            Console.WriteLine($"[PredictionService] Python output: {output}");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            PredictionResult result;

            try
            {
                result = JsonSerializer.Deserialize<PredictionResult>(output, options)
                    ?? new PredictionResult { Symbol = symbol };
            }
            catch
            {
                // Fallback: try parsing as just an array of numbers
                var values = JsonSerializer.Deserialize<List<decimal>>(output, options)
                    ?? new List<decimal>();
                result = new PredictionResult
                {
                    Symbol = symbol,
                    PredictedValues = values
                };
            }

            // Generate future dates if not provided by Python
            if (result.Dates.Count == 0 && result.PredictedValues.Count > 0)
            {
                result.Dates = GenerateFutureDates(lastDate, result.PredictedValues.Count);
            }

            result.Symbol = symbol;
            return result;
        }

        private List<DateTime> GenerateFutureDates(DateTime lastDate, int count)
        {
            var dates = new List<DateTime>();
            var current = lastDate;

            for (int i = 0; i < count; i++)
            {
                current = GetNextBusinessDay(current);
                dates.Add(current);
            }

            return dates;
        }

        private DateTime GetNextBusinessDay(DateTime date)
        {
            var next = date.AddDays(1);
            while (next.DayOfWeek == DayOfWeek.Saturday || next.DayOfWeek == DayOfWeek.Sunday)
            {
                next = next.AddDays(1);
            }
            return next;
        }
    }
}
