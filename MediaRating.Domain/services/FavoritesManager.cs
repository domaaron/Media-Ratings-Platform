using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.services
{
    public class FavoritesManager
    {
        // favorites, no duplicate values
        private readonly HashSet<IMediaEntry> _favorites = new();
        public void AddFavorite(IMediaEntry mediaEntry)
        {
            _favorites.Add(mediaEntry);
        }

        public void RemoveFavorite(IMediaEntry mediaEntry)
        {
            _favorites.Remove(mediaEntry);
        }

        public int CountFavorites()
        {
            return _favorites.Count;
        }

        public IReadOnlyCollection<IMediaEntry> GetAllFavorites()
        {
            return _favorites;
        }
    }
}
