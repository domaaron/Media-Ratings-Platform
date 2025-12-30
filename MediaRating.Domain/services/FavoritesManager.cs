using MediaRatings.Domain.interfaces;
using MediaRatings.Domain.repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.services
{
    public class FavoritesManager : IFavoritesManager
    {
        private readonly IFavoritesRepository _repository;

        public FavoritesManager(IFavoritesRepository repository)
        {
            _repository = repository;
        }

        public async Task AddFavoriteAsync(int userId, int mediaId)
        {
            await _repository.AddFavoriteAsync(userId, mediaId);
        }

        public async Task<bool> RemoveFavoriteAsync(int userId, int mediaId)
        {
            return await _repository.RemoveFavoriteAsync(userId, mediaId);
        }

        public async Task<int> CountFavoritesAsync(int userId)
        {
            var favorites = await _repository.GetFavoritesByUserAsync(userId);
            return favorites.Count;
        }

        public async Task<IReadOnlyCollection<IMediaEntry>> GetAllFavoritesAsync(int userId)
        {
            return await _repository.GetFavoritesByUserAsync(userId);
        }
    }
}
