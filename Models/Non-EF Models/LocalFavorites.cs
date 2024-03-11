namespace Weathered.Models
{
    public class LocalFavorites
    {
        public string stationName { get; set; } = "";
        public int stationLocId { get; set; }

        public LocalFavorites(string stationName, int stationLocId)
        {
            this.stationName = stationName;
            this.stationLocId = stationLocId;
        }
    }
}
