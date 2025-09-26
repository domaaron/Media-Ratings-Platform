using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.interfaces
{
    public interface IFavoritesManager
    {
        void AddFavorite(IMediaEntry mediaEntry);
        void RemoveFavorite(IMediaEntry mediaEntry);
        int CountFavorites();
        IReadOnlyCollection<IMediaEntry> GetAllFavorites();
    }
}
