using MediaRatings.Domain;
using MediaRatings.Domain.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Ratings_Platform.Test.fakes
{
    public class InMemoryRatingManager : IRatingManager
    {
        private readonly Dictionary<int, List<UserRating>> _ratings = new();
        private readonly Dictionary<int, HashSet<int>> _likes = new();

        public async Task AddRatingAsync(UserRating rating)
        {
            if (!_ratings.ContainsKey(rating.User.UserId))
            {
                _ratings[rating.User.UserId] = new List<UserRating>();
            }

            _ratings[rating.User.UserId].Add(rating);
            await Task.CompletedTask;
        }

        public async Task RemoveRatingAsync(UserRating rating)
        {
            if (_ratings.ContainsKey(rating.User.UserId))
            {
                _ratings[rating.User.UserId].Remove(rating);
            }

            await Task.CompletedTask;
        }

        public async Task<IReadOnlyCollection<UserRating>> GetAllRatingsAsync(int userId)
        {
            if (!_ratings.ContainsKey(userId))
            {
                _ratings[userId] = new List<UserRating>();
            }

            return await Task.FromResult(_ratings[userId].ToList());
        }

        public async Task<bool> LikeRatingAsync(UserRating rating, UserAccount user)
        {
            // cannot like own rating
            if (rating.User.UserId == user.UserId)
            {
                return false;
            }

            if (!_likes.ContainsKey(user.UserId))
            {
                _likes[user.UserId] = new HashSet<int>();
            }

            if (_likes[user.UserId].Contains(rating.RatingId))
            {
                return false;
            }

            var added = rating.AddLike(user.UserId);
            if (!added)
            {
                return false;
            }

            _likes[user.UserId].Add(rating.RatingId);

            return await Task.FromResult(true);
        }


        public async Task<IEnumerable<UserRating>> GetRatingHistoryAsync(int userId)
        {
            if (!_ratings.ContainsKey(userId))
            {
                _ratings[userId] = new List<UserRating>();
            }

            return await Task.FromResult(_ratings[userId].OrderBy(r => r.MediaEntry.MediaId));
        }

        public async Task<double> AverageRatingGivenAsync(int userId)
        {
            if (!_ratings.ContainsKey(userId) || !_ratings[userId].Any())
            {
                return await Task.FromResult(0.0);
            }

            return await Task.FromResult(_ratings[userId].Average(r => r.StarValue));
        }

        public async Task<int> CountRatingsAsync(int userId)
        {
            if (!_ratings.ContainsKey(userId))
            {
                _ratings[userId] = new List<UserRating>();
            }

            return await Task.FromResult(_ratings[userId].Count);
        }

        public async Task UpdateRatingAsync(UserRating rating)
        {
            if (_ratings.ContainsKey(rating.User.UserId))
            {
                var existing = _ratings[rating.User.UserId].FirstOrDefault(r => r.RatingId == rating.RatingId);
                if (existing != null)
                {
                    existing.EditRating(existing.User, rating.StarValue, rating.Comment);
                }
            }

            await Task.CompletedTask;
        }

        public async Task<UserRating?> GetRatingByIdAsync(int ratingId)
        {
            foreach (var userRatings in _ratings.Values)
            {
                var rating = userRatings.FirstOrDefault(r => r.RatingId == ratingId);
                if (rating != null)
                {
                    return await Task.FromResult(rating);
                }
            }

            return await Task.FromResult<UserRating?>(null);
        }
    }
}
