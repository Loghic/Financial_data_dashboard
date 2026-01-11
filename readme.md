# Market Data Dashboard

![.NET Build & Test](https://github.com/Loghic/FJFI-school/actions/workflows/dotnet.yml/badge.svg)

**Market Data Dashboard** is a **web application** built in **C# (ASP.NET Core)** that allows users to download, store, visualize, and analyze historical stock market data.
The application demonstrates real-world software engineering practices such as asynchronous data processing, local data persistence, external service integration, modular analytics, and clean architectural separation.

---

## Features

- Download historical stock price data from Alpha Vantage API
- Store market data locally using **SQLite** with Entity Framework Core
- Incrementally update only missing data (smart caching)
- Interactive visualization of time-series price data with **Chart.js**
- Responsive UI with tabbed interface for multiple stock symbols
- Export data to CSV format
- Built-in analytical indicators with detailed explanations
- Technical indicators including:
    - Simple Moving Average (SMA) - 10, 20, 50, 100 periods
    - Exponential Moving Average (EMA) - 10, 20, 50 periods
    - Relative Strength Index (RSI) - 7 and 14 periods
    - Bollinger Bands (20-period with 2 standard deviations)
    - MACD (12, 26, 9 configuration)
- **Asynchronous prediction module** via external Python script with real-time polling
- Chart zoom and pan functionality
- Graceful error handling (network issues, API failures)

---

## Architecture Overview

The application is structured into clearly separated layers:

```
┌─────────────────────────────────────────────────────────┐
│                   Presentation Layer                    │
│              (Razor Pages, Chart.js, DataTables)        │
├─────────────────────────────────────────────────────────┤
│                  Application Services                   │
│  ┌─────────────┐ ┌─────────────────┐ ┌───────────────┐  │
│  │StockService │ │StockDataService │ │PredictionSvc  │  │
│  │  (API)      │ │  (Orchestrator) │ │  (Singleton)  │  │
│  └─────────────┘ └─────────────────┘ └───────────────┘  │
├─────────────────────────────────────────────────────────┤
│                   Data Access Layer                     │
│         StockContext (Entity Framework Core)            │
├─────────────────────────────────────────────────────────┤
│                    SQLite Database                      │
│              (Stocks, PricePoints tables)               │
└─────────────────────────────────────────────────────────┘
          │                                    │
          ▼                                    ▼
   ┌──────────────┐                   ┌──────────────────┐
   │ Alpha Vantage│                   │ Python Script    │
   │     API      │                   │ (predict.py)     │
   └──────────────┘                   └──────────────────┘
```

### Key Architectural Decisions

1. **Singleton PredictionService**: Registered as a singleton to maintain prediction state across HTTP requests, enabling async polling.

2. **IServiceScopeFactory Pattern**: Used in PredictionService to resolve scoped services (DbContext) within background tasks.

3. **Fire-and-Forget with Polling**: Predictions run asynchronously; the UI polls for results every 500ms.

4. **Client-Side Chart Management**: Chart instances and data maps stored globally in JavaScript for dynamic updates.

---

## Project Structure

```
MarketDataDashboard/
├── Data/
│   └── StockContext.cs          # EF Core DbContext
├── Helpers/
│   ├── CsvExporter.cs           # CSV export utility
│   └── TechnicalIndicators.cs   # SMA, EMA, RSI, BB, MACD calculations
├── Models/
│   ├── Stock.cs                 # Stock entity
│   ├── PricePoint.cs            # Price data entity
│   └── PredictionModels.cs      # PredictionInput, PredictionResult
├── Pages/
│   ├── Index.cshtml             # Main dashboard page
│   ├── Index.cshtml.cs          # Page model with handlers
│   ├── Indicators.cshtml        # Indicator explanations page
│   └── Indicators.cshtml.cs     # Indicator info dictionary
├── Services/
│   ├── IStockService.cs         # Stock service interface
│   ├── StockService.cs          # Alpha Vantage API client
│   ├── StockDataService.cs      # Data orchestration (API + DB)
│   └── PredictionService.cs     # Python script integration
├── python/
│   ├── predict.py               # Regression prediction script
│   └── venv/                    # Python virtual environment
├── Program.cs                   # DI configuration, middleware
└── appsettings.json             # Configuration (API key)

MarketDataDashboard.Tests/
├── TechnicalIndicatorsTests.cs  # SMA, EMA, RSI, BB, MACD tests
├── CsvExporterTests.cs          # CSV export tests
├── StockDataServiceTests.cs     # Data service tests with mocking
└── PredictionServiceTests.cs    # Prediction workflow tests
```

---

## Technologies Used

| Category | Technology |
|----------|------------|
| Language | C# 12 / .NET 9 |
| Web Framework | ASP.NET Core Razor Pages |
| Database | SQLite with Entity Framework Core |
| Data Visualization | Chart.js with zoom plugin |
| Data Tables | jQuery DataTables |
| CSS Framework | Bootstrap 5 |
| Async Processing | async/await, Task.Run, ConcurrentDictionary |
| External API | Alpha Vantage (TIME_SERIES_DAILY) |
| Analytics | Python 3 (NumPy, scikit-learn) |
| Content Rendering | Markdig (Markdown to HTML) |
| Data Exchange | JSON (System.Text.Json) |
| Testing | xUnit, Moq, EF Core InMemory |
| CI/CD | GitHub Actions |

---

## Testing

The project includes a comprehensive test suite with **33 unit tests** covering core functionality.

### CI/CD

Tests run automatically on every push and pull request via **GitHub Actions**.

### Run Tests Locally

```bash
cd src/MarketDataDashboard.Tests
dotnet test
```

### Test Coverage

| Component | Tests | Description |
|-----------|-------|-------------|
| **TechnicalIndicators** | 15 | SMA, EMA, RSI, Bollinger Bands, MACD calculations |
| **CsvExporter** | 5 | CSV formatting, headers, culture handling |
| **StockDataService** | 7 | Database operations, incremental updates, case sensitivity |
| **PredictionService** | 6 | Future date generation, business day logic, state management |

### Testing Approach

- **Unit Tests**: Pure function testing for technical indicators and helpers
- **Mocking**: `IStockService` interface mocked with Moq for isolated service testing
- **In-Memory Database**: EF Core InMemory provider for database operation tests
- **Edge Cases**: Empty data, boundary conditions, weekend handling

### Test Technologies

| Package | Purpose |
|---------|---------|
| xUnit | Test framework |
| Moq | Mocking framework |
| Microsoft.EntityFrameworkCore.InMemory | In-memory database for tests |

---

## Analytical Indicators Module

The application includes a dedicated **Analytical Indicators** page accessible from the main navigation.

### Server-Side Calculations (TechnicalIndicators.cs)
- **SMA**: Simple Moving Average with configurable period
- **EMA**: Exponential Moving Average with smoothing factor k = 2/(period+1)
- **RSI**: Relative Strength Index using Wilder's smoothing method
- **Bollinger Bands**: Middle band (SMA) ± 2 standard deviations
- **MACD**: Difference of EMA12 and EMA26 with 9-period signal line

### Client-Side Visualization
- Interactive accordion-based UI
- Lazy-loaded mini charts (rendered on accordion open)
- Markdown descriptions converted to HTML at runtime

---

## Prediction Module

The prediction module demonstrates **cross-language integration** between C# and Python.

### Architecture
```
User clicks "Generate Prediction"
         │
         ▼
┌─────────────────────────────────────────────┐
│  POST /Index?handler=Predict                │
│  └─> PredictionService.StartPrediction()    │
│      └─> Task.Run (background)              │
│          └─> Process.Start(python)          │
│              └─> predict.py (stdin/stdout)  │
│          └─> Store in ConcurrentDictionary  │
└─────────────────────────────────────────────┘
         │
         ▼ (immediate response)
┌─────────────────────────────────────────────┐
│  JavaScript polling (every 500ms)           │
│  GET /?handler=PredictionResult&symbol=...  │
│  └─> Returns null until prediction ready    │
│  └─> Returns {predictedValues, futureDates} │
└─────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────┐
│  Chart updated with future dates            │
│  - X-axis extended with business days       │
│  - Prediction shown as dashed pink line     │
│  - Historical indicators padded with nulls  │
└─────────────────────────────────────────────┘
```

### Features
- **Non-blocking**: UI remains responsive during prediction
- **Visual feedback**: Spinner shown during processing
- **Future dates**: Automatically generates business days (skips weekends)
- **Chart integration**: Prediction line extends beyond historical data

---

## Setup & Running

### Requirements
- .NET 9 SDK
- Internet connection (for Alpha Vantage API)
- Alpha Vantage API key (free at https://www.alphavantage.co/support/#api-key)
- Python 3.x with virtual environment (optional, for predictions)

### Steps

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd MarketDataDashboard
   ```

2. **Configure API key** using .NET User Secrets:
   ```bash
   # Initialize user secrets (only needed once)
   dotnet user-secrets init

   # Set your Alpha Vantage API key
   dotnet user-secrets set "AlphaVantage:ApiKey" "YOUR_API_KEY"
   ```

   > **Note**: User secrets are stored securely outside the project folder and are not committed to git. This is the recommended approach for development.

   **Alternative**: Create `appsettings.Local.json` (gitignored):
   ```json
   {
     "AlphaVantage": {
       "ApiKey": "YOUR_API_KEY"
     }
   }
   ```

3. **Run the application**
   ```bash
   dotnet run --configuration Release
   ```

4. **Open browser** at `https://localhost:5225`

### Enable Predictions (Optional)

```bash
cd bin/Release/net9.0/python
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
pip install numpy scikit-learn
```

---

## API Endpoints (Page Handlers)

| Handler | Method | Description |
|---------|--------|-------------|
| `OnGetAsync` | GET | Load dashboard with all stock data |
| `OnPostUpdateAsync` | POST | Fetch latest data from Alpha Vantage |
| `OnGetDownloadAsync` | GET | Export stock data as CSV |
| `OnPostPredictAsync` | POST | Start background prediction |
| `OnGetPredictionResult` | GET | Poll for prediction results |

---

## Data Storage

- **Stocks table**: Symbol, Name
- **PricePoints table**: Date, Open, High, Low, Close, Volume, StockId (FK)
- **Incremental updates**: Only fetches dates not already in database
- **Supported symbols**: AAPL, MSFT, GOOGL, AMZN, TSLA (configurable)

---

## Future Improvements

- [ ] Support for additional financial instruments (ETFs, indices)
- [ ] Advanced technical indicators (ADX, Stochastic, OBV)
- [ ] Customizable indicator parameters via UI
- [ ] Background scheduled updates (Hosted Service)
- [ ] Improved prediction models (LSTM, Prophet)
- [ ] Signal-based alerts (price targets, indicator thresholds)
- [ ] Server-side indicator caching
- [ ] User authentication and watchlists
- [ ] Docker containerization
- [x] CI/CD pipeline with GitHub Actions

---

## Author

**Matej Michalek**
CTU FNSPE - Programming in C#
