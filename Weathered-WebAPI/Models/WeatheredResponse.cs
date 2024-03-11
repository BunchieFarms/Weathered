using Weathered_Lib.Models;

namespace Weathered_WebAPI.Models
{
    public class WeatheredResponse
    {
        public string StationNumber { get; set; } = "";
        public string StationName { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<WeatherData> Data { get; set; } = [];

        public WeatheredResponse(PastWeekStationData station, DateTime locationDateTime)
        {
            StationNumber = station.StationNumber;
            StationName = station.StationName;
            Latitude = station.Latitude;
            Longitude = station.Longitude;

            foreach (PastWeekData day in station.PastWeekData)
            {
                Data.Add(new WeatherData(day));
            }

            var latestDate = DateOnly.FromDateTime(station.LastUpdate);
            var locationDate = DateOnly.FromDateTime(locationDateTime);
            if (latestDate < locationDate)
            {
                var daysBetween = locationDate.DayNumber - latestDate.DayNumber;
                for (var i = 1; i <= daysBetween; i++)
                {
                    Data.Add(new WeatherData(latestDate.AddDays(i)));
                }
            }
        }

        public WeatheredResponse()
        {
            StationNumber = "-1";
            StationName= "Could not find a nearby weather station.";
        }
    }

    public class WeatherData
    {
        public DateOnly RecordDate { get; set; }
        public bool Is_Forecast { get; set; } = false;
        public double Temp_Max { get; set; }
        public double Temp_Min { get; set; }
        public double Visibility { get; set; }
        public double WindSpeed_Avg { get; set; }
        public double WindSpeed_Gust { get; set; }
        public double Precip_Total { get; set; }
        public string Precip_Type { get; set; } = "";

        public WeatherData(PastWeekData day)
        {
            RecordDate = DateOnly.FromDateTime(day.Date);
            Temp_Max = day.Temp_Max.Value;
            Temp_Min = day.Temp_Min.Value;
            Visibility = day.Visi_Mean.Value;
            WindSpeed_Avg = day.WndSpd_Mean.Value;
            WindSpeed_Gust = day.WndSpd_Gust.Value;
            Precip_Total = day.Precip_Total.Value;
            Precip_Type = day.Temp_Min.Value > 32 ? "rain" : "snow or sleet";
        }

        public WeatherData(DateOnly date, bool isForecast = true)
        {
            RecordDate = date;
            Is_Forecast = isForecast;
        }
    }
}
