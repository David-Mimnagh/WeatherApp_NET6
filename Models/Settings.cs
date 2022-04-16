using System.Collections.Generic;

namespace WeatherApp_NET6.Models
{
    public class Settings
    {
        public string APIKey { get; set; }
        public string CitiesAPIUrl { get; set; }
        public string GeoLocAPIUrl { get; set; }
        public string ForecastAPIUrl { get; set; }

    }
}
