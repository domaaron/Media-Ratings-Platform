using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.interfaces
{
    public interface IFavoritesManager
    {
        Task AddFavoriteAsync(int userId, int mediaId);
        Task<bool> RemoveFavoriteAsync(int userId, int mediaId);
        Task<int> CountFavoritesAsync(int userId);
        Task<IReadOnlyCollection<IMediaEntry>> GetAllFavoritesAsync(int userId);
    }
}
