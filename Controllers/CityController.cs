using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Web;
using WeatherApp_NET6.Models;

namespace WeatherApp_NET6.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CityController : ControllerBase
    {
        readonly ILogger<CityController> _logger;
        static HttpClient _client = new HttpClient();
        readonly Settings _settings;

        public CityController(ILogger<CityController> logger, IOptions<Settings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }
        [HttpGet]
        [Route("GetCities")]
        public async Task<IEnumerable<CityCondensed>> GetCities(string? location = null)
        {
            _logger.Log(LogLevel.Information, "Location: ", location);
            var citySearchBuilder = new UriBuilder(_settings.CitiesAPIUrl);
            var query = HttpUtility.ParseQueryString(citySearchBuilder.Query);
            query["apikey"] = _settings.APIKey;
            if (location == null)
            {
                var resp = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Error, No Location provided"),
                    ReasonPhrase = "Location must be provided"
                };
                _logger.Log(LogLevel.Warning, "No Location found for GetCities call: ", location);
                throw new System.Web.Http.HttpResponseException(resp);
            }
            query["q"] = location;
            citySearchBuilder.Query = query.ToString();

            HttpResponseMessage response = await _client.GetAsync(citySearchBuilder.Uri);

            if (!response.IsSuccessStatusCode)
            {
                var resp = new HttpResponseMessage(response.StatusCode)
                {
                    Content = new StringContent("Error Retrieving cities"),
                    ReasonPhrase = "Bad response from AcuWeather API."
                };
                string errorResponseContent = await response.Content.ReadAsStringAsync();
                _logger.Log(LogLevel.Warning, "Bad response received from Acuweather: ", errorResponseContent);
                throw new System.Web.Http.HttpResponseException(resp);
            }

            string citySearchResponse = await response.Content.ReadAsStringAsync();
            IEnumerable<City> cities = JsonConvert.DeserializeObject<IEnumerable<City>>(citySearchResponse);
            IEnumerable<CityCondensed> briefCities = cities.Select(city => new CityCondensed()
            {
                Key = city.Key,
                Name = $"{city.LocalizedName} - {city.Country.LocalizedName}"
            }).ToArray();

            return briefCities;
        }

    }
}
