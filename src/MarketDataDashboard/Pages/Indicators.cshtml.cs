using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MarketDataDashboard.Pages
{
    public class IndicatorsModel : PageModel
    {
        public Dictionary<string, string> IndicatorsInfo { get; set; } = new Dictionary<string, string>
        {
            { "SMA", @"**Simple Moving Average (SMA)**
- **Calculation:** Average of closing prices over a specified period (e.g., 10, 20, 50 days).
- **Interpretation:** Smooths price data to identify trend direction. Upward slope suggests an uptrend; downward slope suggests a downtrend.
- **Usage:** Often used to identify support/resistance levels or confirm trend reversals. Longer SMA periods reduce sensitivity to short-term price fluctuations." },

            { "EMA", @"**Exponential Moving Average (EMA)**
- **Calculation:** Similar to SMA, but recent prices are weighted more heavily.
- **Interpretation:** More responsive to recent price changes than SMA.
- **Usage:** Useful for short-term trend analysis, crossover strategies (e.g., EMA10 crossing above EMA50 can indicate a buy signal)." },

            { "RSI", @"**Relative Strength Index (RSI)**
- **Calculation:** RSI = 100 - (100 / (1 + RS)), where RS = Average Gain / Average Loss over N periods (commonly 14).
- **Interpretation:** Measures momentum; values >70 indicate overbought conditions, values <30 indicate oversold conditions.
- **Usage:** Identify potential trend reversals or confirm momentum. Often combined with support/resistance or candlestick patterns." },

            { "Bollinger Bands", @"**Bollinger Bands**
- **Calculation:** Consist of a middle SMA line (usually 20-period) and two bands at ±2 standard deviations from the SMA.
- **Interpretation:** Bands expand during high volatility and contract during low volatility. Price touching the upper band can indicate overbought conditions; lower band indicates oversold.
- **Usage:** Detect volatility, potential breakouts, or reversals. Often used with other indicators (e.g., RSI) for confirmation." },

            { "MACD", @"**Moving Average Convergence Divergence (MACD)**
- **Calculation:** Difference between a short-term EMA (e.g., 12-period) and a long-term EMA (e.g., 26-period). A signal line (9-period EMA of MACD) is plotted for crossovers.
- **Interpretation:** Positive MACD indicates upward momentum, negative indicates downward momentum. Crossovers of MACD and signal line generate buy/sell signals.
- **Usage:** Identify trend direction, momentum strength, and potential entry/exit points." },

            { "Open", @"**Open Price**
- **Definition:** Price at which a stock begins trading at the start of the trading session.
- **Usage:** Used in candlestick patterns and intraday analysis to gauge market sentiment at the opening." },

            { "Close", @"**Close Price**
- **Definition:** Price at which a stock finishes trading at the end of the session.
- **Usage:** Often considered the most important price for technical analysis, forming the basis for SMA, EMA, RSI, Bollinger Bands, and candlestick patterns." },

            { "High", @"**High Price**
- **Definition:** Highest price reached during the trading session.
- **Usage:** Useful in identifying resistance levels and price volatility." },

            { "Low", @"**Low Price**
- **Definition:** Lowest price reached during the trading session.
- **Usage:** Useful in identifying support levels and price volatility." },
        };
    }
}

