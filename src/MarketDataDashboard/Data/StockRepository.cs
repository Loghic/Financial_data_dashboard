using Microsoft.EntityFrameworkCore;
using MarketDataDashboard.Models;

namespace MarketDataDashboard.Data
{
  public class StockContext : DbContext
  {
      public StockContext(DbContextOptions<StockContext> options)
        : base(options) { }

      public DbSet<Stock> Stocks { get; set; }
      public DbSet<PricePoint> PricePoints { get; set; }

  }
}


