using System.Runtime.ConstrainedExecution;

namespace WeatherForecast.Models;

public class Location
{
    public string Name { get; set; } = string.Empty;
    // public string Region { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    // public double Lat { get; set; }
    // public double Lon { get; set; }
    // public string Tz_Id { get; set; } = string.Empty;
    public DateTime LocalTime { get; set; }
}