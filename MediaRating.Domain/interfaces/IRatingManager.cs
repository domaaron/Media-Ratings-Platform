using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.interfaces
{
    public interface IRatingManager
    {
        Task AddRatingAsync(UserRating rating);
        Task RemoveRatingAsync(UserRating rating);
        Task<bool> LikeRatingAsync(UserRating ratingToLike, UserAccount likingUser);
        Task<IEnumerable<UserRating>> GetRatingHistoryAsync(int userId);
        Task<double> AverageRatingGivenAsync(int userId);
        Task<int> CountRatingsAsync(int userId);
        Task<IReadOnlyCollection<UserRating>> GetAllRatingsAsync(int userId);
        Task UpdateRatingAsync(UserRating rating);
        Task<UserRating?> GetRatingByIdAsync(int ratingId);
    }
}
