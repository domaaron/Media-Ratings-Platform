using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.repositories
{
    public interface IFavoritesRepository
    {
        Task AddFavoriteAsync(int userId, int mediaId);
        Task<bool> RemoveFavoriteAsync(int userId, int mediaId);
        Task<IReadOnlyCollection<IMediaEntry>> GetFavoritesByUserAsync(int userId);
        Task<bool> IsFavoritesAsync(int userId, int mediaId);
    }
}
