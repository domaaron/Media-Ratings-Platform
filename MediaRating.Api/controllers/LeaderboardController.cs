using MediaRatings.Api.Utils;
using MediaRatings.Domain.services;
using MediaRatings.Infrastructure.repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Api.controllers
{
    /*
    Provides a public leaderboard of top users:
        - Fetches all users from the repository
        - Calculates each user's average rating given and total number of ratings
        - Sorts users by:
            1. Average rating (descending)
            2. Total ratings (descending)
        - Returns the top 5 users as JSON
        - HTTP response codes:
            200 → successful retrieval of leaderboard
    */
    public class LeaderboardController
    {
        private readonly RatingManager _ratingManager;
        private readonly UserRepository _userRepository;

        public LeaderboardController(RatingManager ratingManager, UserRepository userRepository)
        {
            _ratingManager = ratingManager;
            _userRepository = userRepository;
        }

        public async Task GetLeaderboardAsync(HttpListenerContext context)
        {
            var users = await _userRepository.GetAllUsersAsync();
            var leaderboard = new List<object>();

            foreach (var user in users)
            {
                var avg = await _ratingManager.AverageRatingGivenAsync(user.UserId);
                var count = await _ratingManager.CountRatingsAsync(user.UserId);

                leaderboard.Add(new
                {
                    user.UserId,
                    user.Username,
                    AverageScore = avg,
                    TotalRatings = count
                });
            }

            var topUsers = leaderboard
                .OrderByDescending(u => ((dynamic)u).AverageScore)
                .ThenByDescending(u => ((dynamic)u).TotalRatings)
                .Take(5);

            await HttpHelper.WriteJsonAsync(context.Response, 200, topUsers);
        }

    }
}
