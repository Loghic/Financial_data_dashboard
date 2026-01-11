namespace MarketDataDashboard.Models
{
    public class PredictionResult
    {
        public string Symbol { get; set; } = string.Empty;
        public List<DateTime> Dates { get; set; } = new();  // These will be FUTURE dates
        public List<decimal> PredictedValues { get; set; } = new();
    }

    public class PredictionInput
    {
        public string Symbol { get; set; } = string.Empty;
        public List<DateTime> Dates { get; set; } = new();  // Historical dates
        public List<decimal> ClosePrices { get; set; } = new();
    }
}
