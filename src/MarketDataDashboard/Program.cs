using MarketDataDashboard.Services;
using MarketDataDashboard.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Read API key from configuration
var alphaVantageKey = builder.Configuration["AlphaVantage:ApiKey"]
                      ?? throw new InvalidOperationException("Alpha Vantage API key is missing");

// Add services to the container.
builder.Services.AddRazorPages();

// Register HttpClient and StockService with API key and User-Agent
builder.Services.AddHttpClient<IStockService, StockService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
        "(KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36"
    );
})
.AddTypedClient(client => new StockService(client, alphaVantageKey));

builder.Services.AddScoped<StockDataService>();

builder.Services.AddDbContext<StockContext>(options =>
    options.UseSqlite("Data Source=MarketData.db"));

builder.Services.AddSingleton<PredictionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

