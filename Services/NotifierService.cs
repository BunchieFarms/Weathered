using Weathered.Models;

namespace Weathered.Services
{
    public class NotifierService
    {
        public NotifierService() { }

        private List<LocalFavorites> _allLocalFavorites = new List<LocalFavorites>();
        public List<LocalFavorites> AllLocalFavorites
        {
            get
            {
                return _allLocalFavorites;
            }
            set
            {
                _allLocalFavorites = value;
                if (UpdateFavorites != null)
                {
                    UpdateFavorites?.Invoke();
                }
            }
        }

        private LocalFavorites _selectedFavoriteStation;
        public LocalFavorites SelectedFavoriteStation
        {
            get
            {
                return _selectedFavoriteStation;
            }
            set
            {
                _selectedFavoriteStation = value;
                if (SelectFavorite != null)
                {
                    SelectFavorite?.Invoke();
                }
            }
        }

        public event Func<Task> UpdateFavorites;
        public event Func<Task> SelectFavorite;
    }
}
