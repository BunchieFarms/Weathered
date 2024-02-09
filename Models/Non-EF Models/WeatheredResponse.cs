using System.Text.Json;

namespace Weathered.Models
{
    public class WeatheredResponse
    {
        public readonly string StationName;
        public readonly int StationLocId;
        public readonly float Lat;
        public readonly float Lon;
        public readonly List<DaySummary> DaySummaries = new List<DaySummary>();
        public WeatheredResponse()
        {
            StationName = "Unable to find a station close to that location, please try again.";
            StationLocId = -1;
            Lat = -200;
            Lon = -200;
            DaySummaries = new List<DaySummary>();
        }
        public WeatheredResponse(StationLoc station, DateTime locationDateTime)
        {
            StationName = station.Name;
            StationLocId = station.StationLocId;
            Lat = station.Lat;
            Lon = station.Lon;

            var tempData = JsonSerializer.Deserialize<List<DayValue>>(station.PastWeekData.TempData);
            var prcpData = JsonSerializer.Deserialize<List<DayValue>>(station.PastWeekData.PrecipData);

            for (int i = 0; i < prcpData.Count; i++)
            {
                DaySummaries.Add(new DaySummary(prcpData[i], tempData[i]));
            }

            var latestDate = DaySummaries.OrderByDescending(x => x.Date).First().Date;
            var locationDate = DateOnly.FromDateTime(locationDateTime);
            if (latestDate < locationDate)
            {
                var daysBetween = locationDate.DayNumber - latestDate.DayNumber;
                for (var i = 1; i <= daysBetween; i++)
                {
                    DaySummaries.Add(new DaySummary(latestDate.AddDays(i), 99.99f, 9999.9f));
                }
            }
        }
    }

    public class DayValue
    {
        public DateOnly Date { get; set; }
        public float Value { get; set; }
    }

    public class DaySummary
    {
        public readonly DateOnly Date;
        public float Prcp;
        public float Temp;
        public readonly bool isForecast;
        public DaySummary(DayValue _prcp, DayValue _temp)
        {
            Date = _prcp.Date;
            Prcp = _prcp.Value;
            Temp = _temp.Value;
            isForecast = _temp.Value == 9999.9f || _prcp.Value == 99.99f;
        }
        public DaySummary(DateOnly _date, float _prcp, float _temp)
        {
            Date = _date;
            Prcp = _prcp;
            Temp = _temp;
            isForecast = _temp == 9999.9f || _prcp == 99.99f;
        }
    }
}
