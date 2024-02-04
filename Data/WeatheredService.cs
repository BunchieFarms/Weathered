using GoogleApi;
using GoogleApi.Entities.Maps.Geocoding.Address.Request;
using GoogleApi.Entities.Maps.Geocoding;
using Microsoft.EntityFrameworkCore;
using Weathered.Models;
using GoogleApi.Entities.Common;
using System.Globalization;
using System.Text.Json;

namespace Weathered.Data
{
    public class WeatheredService
    {
        private readonly IConfiguration _config;
        private readonly WeatheredContext _context;
        private readonly GoogleMaps.Geocode.AddressGeocodeApi _geocodeApi;
        private readonly HttpClient _client;
        public WeatheredService(WeatheredContext context, GoogleMaps.Geocode.AddressGeocodeApi geocodeApi, IConfiguration config, HttpClient client)
        {
            _context = context;
            _geocodeApi = geocodeApi;
            _config = config;
            _client = client;
        }
        public async Task<WeatheredResponse> GetWeatheredResponse(string location)
        {
            WeatheredResponse pastWeatherResponse = new WeatheredResponse();
            AddressGeocodeRequest geo = new AddressGeocodeRequest();
            geo.Key = _config["GeocodingKey"];
            geo.Address = location;
            GeocodeResponse res = await _geocodeApi.QueryAsync(geo);
            var resResults = res.Results.Any() ? res.Results.First() : null;
            if (resResults == null)
                return pastWeatherResponse;

            StationLoc nearestStation = GetNearestStation(resResults.Geometry.Location);
            pastWeatherResponse = new WeatheredResponse(nearestStation);

            //List<DaySummary> forecastDaySummaries = pastWeatherResponse.DaySummaries.Where(x => x.isForecast).ToList();
            //if (forecastDaySummaries.Count > 0)
            //{
            //    var histForecast = GetHistoricalForecast(nearestStation.Lat, nearestStation.Lon, forecastDaySummaries[0].Date);
            //    for (var i = 0; i < forecastDaySummaries.Count; i++)
            //    {
            //        pastWeatherResponse.DaySummaries.First(x => x.Date == forecastDaySummaries[i].Date).Temp = histForecast[i].temperatureMax;
            //        pastWeatherResponse.DaySummaries.First(x => x.Date == forecastDaySummaries[i].Date).Prcp = histForecast[i].precipAccumulation;
            //    }
            //}

            return pastWeatherResponse;
        }

        public Datum[] GetHistoricalForecast(float lat, float lon, DateOnly date)
        {
            var secondsAgo = (DateTime.Now.Date - date.ToDateTime(TimeOnly.MinValue).Date).TotalDays * 86400;
            string reqStr = $"https://api.pirateweather.net/forecast/{_config["PirateApiKey"]}/{lat},{lon},-{secondsAgo}?exclude=minutely,alerts,currently,hourly&units=us";
            var apiResponse = _client.GetFromJsonAsync<PirateForecastResponse>(reqStr);
            return apiResponse.Result.daily.data;
        }

        private StationLoc GetNearestStation(Coordinate coords)
        {
            double stepper = 0;
            List<StationLoc> nearbyStations = new List<StationLoc>();
            while (nearbyStations.Count() == 0)
            {
                stepper += .5;
                nearbyStations = GetNearbyStations(coords, stepper);
            }
            double nearestDistance = 100;
            StationLoc nearestLoc = nearbyStations[0];
            foreach (StationLoc station in nearbyStations)
            {
                var currDistance = Math.Abs(station.Lat - coords.Latitude) + Math.Abs(station.Lon - coords.Longitude);
                if (currDistance <= nearestDistance)
                {
                    nearestDistance = currDistance;
                    nearestLoc = station;
                }
            }
            return nearestLoc;
        }

        private List<StationLoc> GetNearbyStations(Coordinate coords, double size)
        {
            return _context.StationLocs.Include(x => x.PastWeekData)
                .Where(x => x.Lat >= coords.Latitude - size && x.Lat <= coords.Latitude + size &&
                            x.Lon >= coords.Longitude - size && x.Lon <= coords.Longitude + size).ToList();
        }
    }


    public class PirateForecastResponse
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
        public Daily daily { get; set; }
    }

    public class Daily
    {
        public string summary { get; set; }
        public string icon { get; set; }
        public Datum[] data { get; set; }
    }

    public class Datum
    {
        public int time { get; set; }
        public string icon { get; set; }
        public string summary { get; set; }
        public float precipAccumulation { get; set; }
        public string precipType { get; set; }
        public float temperatureHigh { get; set; }
        public float temperatureLow { get; set; }
        public float apparentTemperatureHigh { get; set; }
        public float apparentTemperatureLow { get; set; }
        public float temperatureMin { get; set; }
        public int temperatureMinTime { get; set; }
        public float temperatureMax { get; set; }
        public int temperatureMaxTime { get; set; }
        public float apparentTemperatureMin { get; set; }
        public int apparentTemperatureMinTime { get; set; }
        public float apparentTemperatureMax { get; set; }
        public int apparentTemperatureMaxTime { get; set; }
    }


    public class WeatheredResponse
    {
        public readonly string StationName;
        public readonly int StationLocId;
        public readonly float Lat;
        public readonly float Lon;
        public readonly List<DaySummary> DaySummaries = new List<DaySummary>();
        public WeatheredResponse(StationLoc station)
        {
            StationName = station.Name;
            StationLocId = station.StationLocId;
            Lat = station.Lat;
            Lon = station.Lon;

            var tempData = Array.ConvertAll(station.PastWeekData.TempData.Split(','), float.Parse);
            var prcpData = Array.ConvertAll(station.PastWeekData.PrecipData.Split(','), float.Parse);
            var daysAgo = -7;

            for (int i = 0; i < 7; i++)
            {
                DaySummaries?.Add(new DaySummary(DateTime.Now.AddDays(daysAgo), prcpData[i], tempData[i]));
                daysAgo++;
            }
        }
        public WeatheredResponse()
        {
            StationName = "Geocoding Failed.";
            StationLocId = -1;
            Lat = -1;
            Lon = -1;
            DaySummaries = new List<DaySummary>();
        }
    }

    public class DaySummary
    {
        public readonly DateOnly Date;
        public float Prcp;
        public float Temp;
        public readonly bool isForecast;
        public DaySummary(DateTime _date, float _prcp, float _temp)
        {
            Date = DateOnly.FromDateTime(_date);
            Prcp = _prcp;
            Temp = _temp;
            isForecast = _temp == 9999.9f || _prcp == 99.99f;
        }
    }

}
