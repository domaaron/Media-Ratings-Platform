using MediaRatings.Domain.interfaces;
using MediaRatings.Domain.repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.services
{
    public class RatingManager : IRatingManager
    {
        private readonly IRatingRepository _repository;

        public RatingManager(IRatingRepository repository)
        {
            _repository = repository;
        }

        public async Task AddRatingAsync(UserRating rating)
        {
            if (rating.User == null)
            {
                throw new ArgumentException("User must be set for the rating.");
            }

            if (rating.StarValue < 1 || rating.StarValue > 5)
            {
                throw new ArgumentOutOfRangeException("Invalid rating: Stars must be between 1 and 5.");
            }

            await _repository.AddRatingAsync(rating);
        }

        public async Task RemoveRatingAsync(UserRating rating)
        {
            await _repository.DeleteRatingAsync(rating.RatingId);
        }

        public async Task<bool> LikeRatingAsync(UserRating ratingToLike, UserAccount likingUser)
        {
            // cannot like own rating
            if (ratingToLike.User.UserId == likingUser.UserId)
            {
                return false;
            }

            // cannot like twice the same rating
            if (!ratingToLike.AddLike(likingUser.UserId))
            {
                return false;
            }

            await _repository.UpdateRatingAsync(ratingToLike);
            return true;
        }

        public async Task<IEnumerable<UserRating>> GetRatingHistoryAsync(int userId)
        {
            var ratings = await _repository.GetRatingByUserAsync(userId);
            return ratings.OrderByDescending(rating => rating.RatingTimestamp);
        }

        public async Task<double> AverageRatingGivenAsync(int userId)
        {
            var ratings = await _repository.GetRatingByUserAsync(userId);
            if (!ratings.Any())
            {
                return 0;
            }

            return ratings.Average(r => r.StarValue);
        }

        public async Task<int> CountRatingsAsync(int userId)
        {
            var ratings = await _repository.GetRatingByUserAsync(userId);
            return ratings.Count;
        }

        public async Task<IReadOnlyCollection<UserRating>> GetAllRatingsAsync(int userId)
        {
            return await _repository.GetRatingByUserAsync(userId);
        }

        public async Task UpdateRatingAsync(UserRating rating)
        {
            await _repository.UpdateRatingAsync(rating);
        }

        public async Task<UserRating?> GetRatingByIdAsync(int ratingId)
        {
            return await _repository.GetRatingByIdAsync(ratingId);
        }

    }
}
