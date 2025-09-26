using MediaRatings.Domain.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.services
{
    public class RatingManager : IRatingManager
    {
        // ratings created by own user
        private readonly List<UserRating> _ratings = new();
        private readonly UserAccount _owner;

        public RatingManager(UserAccount owner)
        {
            _owner = owner;
        }

        public void AddRating(UserRating rating)
        {
            if (rating.StarValue < 1 || rating.StarValue > 5)
            {
                throw new ArgumentOutOfRangeException("Invalid rating: Stars must be between 1 and 5.");
            }

            _ratings.Add(rating);
        }

        public void RemoveRating(UserRating rating)
        {
            _ratings.Remove(rating);
        }

        public bool LikeRating(UserRating ratingToLike, UserAccount likingUser)
        {
            // cannot like own rating
            if (ratingToLike.User == likingUser)
            {
                return false;
            }

            // cannot like twice the same rating
            if (ratingToLike.LikedBy.Contains(likingUser.UserId))
            {
                return false;
            }

            ratingToLike.LikedBy.Add(likingUser.UserId);
            return true;
        }

        public IEnumerable<UserRating> GetRatingHistory()
        {
            return _ratings.OrderByDescending(rating => rating.StarValue);
        }

        public double AverageRatingGiven()
        {
            if (!_ratings.Any())
            {
                return 0;
            }

            return _ratings.Average(r => r.StarValue);
        }

        public int CountRatings()
        {
            return _ratings.Count();
        }

        public IReadOnlyCollection<UserRating> GetAllRatings()
        {
            return _ratings.AsReadOnly();
        }

    }
}
