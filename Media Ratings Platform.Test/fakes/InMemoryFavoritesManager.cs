using MediaRatings.Domain;
using MediaRatings.Domain.interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Media_Ratings_Platform.Test.fakes
{
    public class InMemoryFavoritesManager : IFavoritesManager
    {
        private readonly Dictionary<int, List<IMediaEntry>> _favorites = new();

        public async Task AddFavoriteAsync(int userId, int mediaId)
        {
            if (!_favorites.ContainsKey(userId))
            {
                _favorites[userId] = new List<IMediaEntry>();
            }

            var media = new Movie(
                createdBy: userId,
                title: "Dummy",
                description: "",
                releaseYear: 0,
                genres: new List<Genres>(),
                ageRestriction: 0,
                mediaId: mediaId
            );

            if (!_favorites[userId].Any(m => m.MediaId == mediaId))
            {
                _favorites[userId].Add(media);
            }

            await Task.CompletedTask;
        }

        public async Task<bool> RemoveFavoriteAsync(int userId, int mediaId)
        {
            if (!_favorites.ContainsKey(userId))
            {
                return false;
            }

            var media = _favorites[userId].FirstOrDefault(m => m.MediaId == mediaId);
            if (media != null)
            {
                _favorites[userId].Remove(media);
                return true;
            }

            return false;
        }

        public async Task<IReadOnlyCollection<IMediaEntry>> GetAllFavoritesAsync(int userId)
        {
            if (!_favorites.ContainsKey(userId))
            {
                _favorites[userId] = new List<IMediaEntry>();
            }

            return await Task.FromResult(_favorites[userId].AsReadOnly());
        }

        public async Task<int> CountFavoritesAsync(int userId)
        {
            if (!_favorites.ContainsKey(userId))
            {
                _favorites[userId] = new List<IMediaEntry>();
            }

            return await Task.FromResult(_favorites[userId].Count);
        }
    }
}
