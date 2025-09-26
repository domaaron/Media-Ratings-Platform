using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.interfaces
{
    public interface IRatingManager
    {
        void AddRating(UserRating rating);
        void RemoveRating(UserRating rating);
        bool LikeRating(UserRating ratingToLike, UserAccount likingUser);
        IEnumerable<UserRating> GetRatingHistory();
        double AverageRatingGiven();
        int CountRatings();
        IReadOnlyCollection<UserRating> GetAllRatings();
    }
}
