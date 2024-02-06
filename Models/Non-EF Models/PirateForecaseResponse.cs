namespace Weathered.Models
{
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
}
