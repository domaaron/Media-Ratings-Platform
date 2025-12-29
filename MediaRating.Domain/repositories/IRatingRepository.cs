using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.repositories
{
    public interface IRatingRepository
    {
        Task<int> AddRatingAsync(UserRating rating);
        Task UpdateRatingAsync(UserRating rating);
        Task DeleteRatingAsync(int ratingId);
        Task<UserRating?> GetRatingByIdAsync(int ratingId);
        Task<IReadOnlyCollection<UserRating>> GetRatingByUserAsync(int userId);
        Task<IReadOnlyCollection<UserRating>> GetRatingByMediaAsync(int userId);
    }
}
