using MarketDataDashboard.Models;

namespace MarketDataDashboard.Helpers{
  public static class TechnicalIndicators
  {
      // Simple Moving Average
      public static List<decimal?> SMA(List<PricePoint> data, int period)
      {
          var result = new List<decimal?>();
          for (int i = 0; i < data.Count; i++)
          {
              if (i + 1 < period)
              {
                  result.Add(null);
                  continue;
              }
              decimal sum = 0;
              for (int j = i + 1 - period; j <= i; j++)
                  sum += data[j].Close;
              result.Add(sum / period);
          }
          return result;
      }

      // Exponential Moving Average
      public static List<decimal?> EMA(List<PricePoint> data, int period)
      {
          var result = new List<decimal?>();
          decimal? prev = null;
          decimal k = 2m / (period + 1);
          for (int i = 0; i < data.Count; i++)
          {
              if (i + 1 < period)
              {
                  result.Add(null);
                  continue;
              }

              if (prev == null)
              {
                  decimal sum = 0;
                  for (int j = i + 1 - period; j <= i; j++)
                      sum += data[j].Close;
                  prev = sum / period;
              }
              else
              {
                  prev = (data[i].Close - prev) * k + prev;
              }
              result.Add(prev);
          }
          return result;
      }

      // RSI (Relative Strength Index)
      public static List<decimal?> RSI(List<PricePoint> data, int period)
      {
          var result = new List<decimal?>();
          decimal gain = 0, loss = 0;

          for (int i = 0; i < data.Count; i++)
          {
              if (i == 0)
              {
                  result.Add(null);
                  continue;
              }

              var change = data[i].Close - data[i - 1].Close;
              gain += Math.Max(change, 0);
              loss += Math.Max(-change, 0);

              if (i < period)
              {
                  result.Add(null);
              }
              else if (i == period)
              {
                  decimal avgGain = gain / period;
                  decimal avgLoss = loss / period;
                  decimal rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
                  result.Add(100 - 100 / (1 + rs));
              }
              else
              {
                  decimal lastRSI = result[i - 1] ?? 0;
                  decimal avgGain = (gain * (period - 1) + Math.Max(change, 0)) / period;
                  decimal avgLoss = (loss * (period - 1) + Math.Max(-change, 0)) / period;
                  decimal rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
                  result.Add(100 - 100 / (1 + rs));
              }
          }
          return result;
      }

      // Bollinger Bands
      public static (List<decimal?> Upper, List<decimal?> Lower) BollingerBands(List<PricePoint> data, int period, decimal multiplier = 2)
      {
          var upper = new List<decimal?>();
          var lower = new List<decimal?>();
          for (int i = 0; i < data.Count; i++)
          {
              if (i + 1 < period)
              {
                  upper.Add(null);
                  lower.Add(null);
                  continue;
              }

              decimal sum = 0;
              for (int j = i + 1 - period; j <= i; j++)
                  sum += data[j].Close;
              decimal sma = sum / period;

              decimal variance = 0;
              for (int j = i + 1 - period; j <= i; j++)
                  variance += (data[j].Close - sma) * (data[j].Close - sma);
              decimal stddev = (decimal)Math.Sqrt((double)(variance / period));

              upper.Add(sma + multiplier * stddev);
              lower.Add(sma - multiplier * stddev);
          }
          return (upper, lower);
      }

      // MACD (12,26,9)
      public static (List<decimal?> MACD, List<decimal?> Signal) MACD(List<PricePoint> data)
      {
          var ema12 = EMA(data, 12);
          var ema26 = EMA(data, 26);
          var macd = new List<decimal?>();
          for (int i = 0; i < data.Count; i++)
          {
              if (ema12[i] == null || ema26[i] == null)
                  macd.Add(null);
              else
                  macd.Add(ema12[i] - ema26[i]);
          }
          var signal = EMA(macd.Select((v, i) => new PricePoint { Close = v ?? 0 }).ToList(), 9);
          return (macd, signal);
      }
  }
}
