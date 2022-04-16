using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using WeatherApp_NET6.Models;

namespace WeatherApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        static HttpClient client = new HttpClient();
        readonly Settings _settings;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IOptions<Settings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        [HttpGet]
        [Route("GetWeatherInfo")]
        public async Task<WeatherForecast> GetWeatherInfo(string? latitude = null, string? longitude = null, string? cityKey = null,  bool useMetric = false)
        {
            _logger.Log(LogLevel.Information, "Getting Weather Info for city key: ", cityKey);
            if ((String.IsNullOrEmpty(latitude) || String.IsNullOrEmpty(longitude)))
            {
                if (String.IsNullOrEmpty(cityKey))
                {
                    var resp = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent("Error, No CityKey or Lat,Lon provided"),
                        ReasonPhrase = "City Key or Lat,Lon must be provided"
                    };
                    _logger.Log(LogLevel.Warning, "Invalid parameters passed to GetWeatherInfo call.");
                    throw new System.Web.Http.HttpResponseException(resp);
                }
            }
            var locationName = cityKey;
            if ((String.IsNullOrEmpty(cityKey) || cityKey == "null"))
            {
                var geoLocationBuilder = new UriBuilder(_settings.GeoLocAPIUrl);
                var geoLocQuery = HttpUtility.ParseQueryString(geoLocationBuilder.Query);
                geoLocQuery["apikey"] = _settings.APIKey;
                geoLocQuery["q"] = $"{latitude},{longitude}";
                geoLocationBuilder.Query = geoLocQuery.ToString();
                HttpResponseMessage geoLocResponse = await client.GetAsync(geoLocationBuilder.Uri);
                if (!geoLocResponse.IsSuccessStatusCode)
                {
                    string responseErrorMessage = await geoLocResponse.Content.ReadAsStringAsync();
                    var resp = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent("Error, Bad response from Acuweather API when getting geolocation data"),
                        ReasonPhrase = responseErrorMessage
                    };
                    _logger.Log(LogLevel.Warning, "Error, Bad response from Acuweather API when getting geolocation data");
                    throw new System.Web.Http.HttpResponseException(resp);

                }
                string geoLocationResponseContent = await geoLocResponse.Content.ReadAsStringAsync();
                GeoLocation geoLocationInfo = JsonConvert.DeserializeObject<GeoLocation>(geoLocationResponseContent);
                cityKey = geoLocationInfo.Key;
                locationName = $"{geoLocationInfo.LocalizedName} - {geoLocationInfo.Country.LocalizedName}";
            }

            var dailyForecastBuilder = new UriBuilder($"{_settings.ForecastAPIUrl}{cityKey}");
            var query = HttpUtility.ParseQueryString(dailyForecastBuilder.Query);
            query["apikey"] = _settings.APIKey;
            query["metric"] = useMetric ? "true" : "false";

            dailyForecastBuilder.Query = query.ToString();
            HttpResponseMessage response = await client.GetAsync(dailyForecastBuilder.Uri);
            if (!response.IsSuccessStatusCode)
            {
                string responseErrorMessage = await response.Content.ReadAsStringAsync();
                var resp = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Error, Bad response from Acuweather API"),
                    ReasonPhrase = responseErrorMessage
                };
                _logger.Log(LogLevel.Warning, "Error, Bad response from Acuweather API");
                throw new System.Web.Http.HttpResponseException(resp);

            }
            Forecast oneDayForecast;
            string oneDayForecastResponse = await response.Content.ReadAsStringAsync();
            try
            {
                oneDayForecast = JsonConvert.DeserializeObject<Forecast>(oneDayForecastResponse);
            }
            catch (Exception e)
            {
                var resp = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Error, Bad response from Acuweather API"),
                    ReasonPhrase = e.Message
                };
                throw new System.Web.Http.HttpResponseException(resp);
            }
            DailyForecast dailyForecast = oneDayForecast.DailyForecasts.First(); // currently only support 1 day forecast due to api key limitations
            return new WeatherForecast()
            {
                Date = dailyForecast.Date != null ? dailyForecast.Date.Value.ToShortDateString() : DateTime.Today.ToShortDateString(),
                LocationName = locationName,
                summaryText = oneDayForecast.Headline.Text,
                forecastLink = oneDayForecast.Headline.Link,
                temperatureMin = $"{dailyForecast.Temperature.Minimum.Value}{dailyForecast.Temperature.Minimum.Unit}",
                temperatureMax = $"{dailyForecast.Temperature.Maximum.Value}{dailyForecast.Temperature.Maximum.Unit}",
                percipitation = dailyForecast.Day.HasPrecipitation ? $"{dailyForecast.Day.PrecipitationIntensity} {dailyForecast.Day.PrecipitationType}" : "No Precipitation"
            };
        }
    }
}
