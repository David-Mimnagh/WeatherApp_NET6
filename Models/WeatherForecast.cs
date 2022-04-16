namespace WeatherApp_NET6.Models
{
    public class WeatherForecast
    {
        public string Date { get; set; }
        public string LocationName { get; set; }
        public string temperatureMin { get; set; }
        public string temperatureMax { get; set; }
        public string summaryText { get; set; }
        public string percipitation { get; set; }
        public string forecastLink { get; set; }
    }
}