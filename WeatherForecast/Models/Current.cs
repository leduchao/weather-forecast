namespace WeatherForecast.Models;

public class Current
{
  public double Temp_C { get; set; }
  public Condition Condition { get; set; } = new Condition();
  // public double Wind_Mph { get; set; }
  public double Wind_Kph { get; set; }
  public int Humidity { get; set; }
}
