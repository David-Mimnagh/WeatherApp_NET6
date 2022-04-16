using System.Collections.Generic;

namespace WeatherApp_NET6.Models
{
    public class City
    {
        public int Version { get; set; }
        public string Key { get; set; }
        public string Type { get; set; }
        public int Rank { get; set; }
        public string LocalizedName { get; set; }
        public string EnglishName { get; set; }
        public string PrimaryPostalCode { get; set; }
        public Region Region { get; set; }
        public Country Country { get; set; }
        public AdministrativeArea AdministrativeArea { get; set; }
        public TimeZone TimeZone { get; set; }
        public GeoPosition GeoPosition { get; set; }
        public bool IsAlias { get; set; }
        public List<SupplementalAdminArea> SupplementalAdminAreas { get; set; }
        public List<string> DataSets { get; set; }
    }
    public class CityCondensed
    {
        public string Key { get; set; }
        public string Name { get; set; }
    }
}