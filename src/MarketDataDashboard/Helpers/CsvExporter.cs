using System.Globalization;
using System.Text;
using MarketDataDashboard.Models;

namespace MarketDataDashboard.Helpers
{
    public static class CsvExporter
    {
        public static string Export(IEnumerable<PricePoint> data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Date,Open,High,Low,Close,Volume");

            foreach (var p in data)
            {
                sb.AppendLine(
                    $"{p.Date:yyyy-MM-dd}," +
                    $"{p.Open.ToString("F3", CultureInfo.InvariantCulture)}," +
                    $"{p.High.ToString("F3", CultureInfo.InvariantCulture)}," +
                    $"{p.Low.ToString("F3", CultureInfo.InvariantCulture)}," +
                    $"{p.Close.ToString("F3", CultureInfo.InvariantCulture)}," +
                    $"{p.Volume.ToString(CultureInfo.InvariantCulture)}"
                );
            }

            return sb.ToString();
        }
    }
}

