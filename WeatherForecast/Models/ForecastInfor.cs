namespace WeatherForecast.Models;

public class ForecastInfor
{
    public DateOnly Date { get; set; }

    public Day Day { get; set; } = new Day();

    public List<HourInfo> Hour { get; set; } = [];
}
