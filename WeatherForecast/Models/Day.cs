namespace WeatherForecast.Models;

public class Day
{
  public double AvgTemp_C { get; set; }
  // public double MaxWind_Mph { get; set; }
  public double MaxWind_Kph { get; set; }
  public double AvgHumidity { get; set; }
  public Condition Condition { get; set; } = new Condition();
}