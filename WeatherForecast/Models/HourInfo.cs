namespace WeatherForecast.Models
{
    public class HourInfo
    {
        public DateTime Time { get; set; }
        public double Temp_C { get; set; }
        public Condition Condition { get; set; } = new();
        public double Wind_Kph { get; set; }
        public int Humidity { get; set; }
    }
}