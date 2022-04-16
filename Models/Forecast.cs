using System.Collections.Generic;

namespace WeatherApp_NET6.Models
{
    public class Forecast
    {
        public Headline Headline { get; set; }
        public List<DailyForecast> DailyForecasts { get; set; }
    }
}








