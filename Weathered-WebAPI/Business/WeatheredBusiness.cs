using GoogleApi.Entities.Common.Extensions;
using GoogleApi.Entities.Maps.Geocoding.Address.Request;
using GoogleApi.Entities.Maps.Geocoding;
using GoogleApi.Entities.Maps.TimeZone.Request;
using GoogleApi.Entities.Maps.TimeZone.Response;
using GoogleApi;
using GoogleApi.Entities.Common;
using Weathered_Lib.Models;
using Weathered_Lib.Mongo;
using MongoDB.Driver;
using PirateWeather_DotNetLib;
using static PirateWeather_DotNetLib.Enums;

namespace Weathered_WebAPI.Business
{
    public class WeatheredBusiness
    {
        private readonly IConfiguration _config;
        private readonly GoogleMaps.Geocode.AddressGeocodeApi _geocodeApi;
        private readonly GoogleMaps.TimeZoneApi _timezoneApi;
        private readonly HttpClient _client;
        public WeatheredBusiness(GoogleMaps.Geocode.AddressGeocodeApi geocodeApi, IConfiguration config, HttpClient client, GoogleMaps.TimeZoneApi timezoneApi)
        {
            _geocodeApi = geocodeApi;
            _config = config;
            _client = client;
            _timezoneApi = timezoneApi;
        }

        public async Task<WeatheredResponse> GetWeatheredResponse(WeatheredRequest req)
        {
            if (req.Location != "")
                return await GetWeatheredResponseByLocation(req.Location);
            else
                return await GetWeatheredResponseByStationNumber(req.StationNumber);
        }

        private async Task<WeatheredResponse> GetWeatheredResponseByStationNumber(string stationNumber)
        {
            PastWeekStationData station = MongoBase.PastStationDataColl.AsQueryable().First(x => x.StationNumber == stationNumber);
            var locationTime = await GetLocationTime(new Coordinate(station.Latitude, station.Longitude));
            DateTime locationDateTime = DateTimeExtension.epoch.AddSeconds((double)(DateTime.UtcNow.DateTimeToUnixTimestamp() + locationTime.RawOffSet + locationTime.OffSet));
            return new WeatheredResponse(station, locationDateTime);
        }

        private async Task<WeatheredResponse> GetWeatheredResponseByLocation(string location)
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

            PastWeekStationData nearestStation = GetNearestStation(resResults.Geometry.Location);
            pastWeatherResponse = new WeatheredResponse(nearestStation, locationDateTime);

            return pastWeatherResponse;
        }

        public async Task<List<DailyData>> GetHistoricalForecast(PirateWeatheredRequest pReq)
        {
            var daysAgo = (int)(DateTime.Now.Date - pReq.Date.ToDateTime(TimeOnly.MinValue).Date).TotalDays;
            ForecastRequest req = new ForecastRequest
            {
                ApiKey = _config["PirateApiKey"],
                Location = new Location(pReq.Latitude, pReq.Longitude),
                Time = new Time(daysAgo),
                Include = [DataGroup.Daily]
            };
            var res = await PirateForecast.GetAsync(req);
            return res.Daily.Data.ToList();
        }

        private async Task<TimeZoneResponse> GetLocationTime(Coordinate coords)
        {
            TimeZoneRequest tzReq = new TimeZoneRequest { Key = _config["GeocodingKey"], Location = coords, TimeStamp = DateTime.UtcNow };
            return await _timezoneApi.QueryAsync(tzReq);
        }


        private PastWeekStationData GetNearestStation(Coordinate coords)
        {
            double stepper = 0;
            List<PastWeekStationData> nearbyStations = new List<PastWeekStationData>();
            while (nearbyStations.Count() == 0)
            {
                stepper += .5;
                nearbyStations = GetNearbyStations(coords, stepper);
            }
            double nearestDistance = 100;
            PastWeekStationData nearestLoc = nearbyStations.First();
            foreach (PastWeekStationData station in nearbyStations)
            {
                var currDistance = Math.Abs(station.Latitude - coords.Latitude) + Math.Abs(station.Longitude - coords.Longitude);
                if (currDistance <= nearestDistance)
                {
                    nearestDistance = currDistance;
                    nearestLoc = station;
                }
            }
            return nearestLoc;
        }

        private List<PastWeekStationData> GetNearbyStations(Coordinate coords, double size)
        {
            return MongoBase.PastStationDataColl.AsQueryable()
                .Where(x => x.Latitude >= coords.Latitude - size && x.Latitude <= coords.Latitude + size &&
                            x.Longitude >= coords.Longitude - size && x.Longitude <= coords.Longitude + size).ToList();
        }
    }
}
