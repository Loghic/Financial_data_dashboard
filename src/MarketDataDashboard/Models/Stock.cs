namespace MarketDataDashboard.Models
{
  public class Stock
  {
    public int Id { get; set; }
    public required string Symbol { get; set; }
    public required string Name { get; set; }
  }
}
