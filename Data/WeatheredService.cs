using GoogleApi;
using GoogleApi.Entities.Maps.Geocoding.Address.Request;
using GoogleApi.Entities.Maps.Geocoding;
using Microsoft.EntityFrameworkCore;
using Weathered.Models;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Common.Extensions;
using GoogleApi.Entities.Maps.TimeZone.Request;
using GoogleApi.Entities.Maps.TimeZone.Response;

namespace Weathered.Data
{
    public class WeatheredService
    {
        private readonly IConfiguration _config;
        private readonly WeatheredContext _context;
        private readonly GoogleMaps.Geocode.AddressGeocodeApi _geocodeApi;
        private readonly GoogleMaps.TimeZoneApi _timezoneApi;
        private readonly HttpClient _client;
        public WeatheredService(WeatheredContext context, GoogleMaps.Geocode.AddressGeocodeApi geocodeApi, IConfiguration config, HttpClient client, GoogleMaps.TimeZoneApi timezoneApi)
        {
            _context = context;
            _geocodeApi = geocodeApi;
            _config = config;
            _client = client;
            _timezoneApi = timezoneApi;
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

            var locationTime = await GetLocationTime(resResults.Geometry.Location);
            DateTime locationDateTime = DateTimeExtension.epoch.AddSeconds((double)(DateTime.UtcNow.DateTimeToUnixTimestamp() + locationTime.RawOffSet + locationTime.OffSet));

            StationLoc nearestStation = GetNearestStation(resResults.Geometry.Location);
            pastWeatherResponse = new WeatheredResponse(nearestStation, locationDateTime);


            return pastWeatherResponse;
        }

        public Datum[] GetHistoricalForecast(float lat, float lon, DateOnly date)
        {
            var secondsAgo = (DateTime.Now.Date - date.ToDateTime(TimeOnly.MinValue).Date).TotalDays * 86400;
            string reqStr = $"https://api.pirateweather.net/forecast/{_config["PirateApiKey"]}/{lat},{lon},{(secondsAgo == 0 ? null : secondsAgo * -1)}?exclude=minutely,alerts,currently,hourly&units=us";
            var apiResponse = _client.GetFromJsonAsync<PirateForecastResponse>(reqStr);
            return apiResponse.Result.daily.data;
        }

        public async Task<TimeZoneResponse> GetLocationTime(Coordinate coords)
        {
            TimeZoneRequest tzReq = new TimeZoneRequest { Key = _config["GeocodingKey"], Location = coords, TimeStamp = DateTime.UtcNow };
            return await _timezoneApi.QueryAsync(tzReq);
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
}
