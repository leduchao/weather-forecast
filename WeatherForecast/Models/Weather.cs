using WeatherForecast.Models;

namespace WeatherForecast.Models;

public class Weather
{
  public Location Location { get; set; } = new Location();
  public Current Current { get; set; } = new Current();
  public Forecast Forecast { get; set; } = new Forecast();
}
