using Media_Ratings_Platform.services;
using MediaRatings.Api.Utils;
using MediaRatings.Domain;
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
    public class UserController
    {
        private readonly JwtService _jwtService;
        private readonly UserRepository _userRepository;
        private readonly RatingManager _ratingManager;
        private readonly FavoritesManager _favoritesManager;

        public UserController(JwtService jwtService, UserRepository userRepository, RatingManager ratingManager, FavoritesManager favoritesManager)
        {
            _jwtService = jwtService;
            _userRepository = userRepository;
            _ratingManager = ratingManager;
            _favoritesManager = favoritesManager;
        }

        public async Task GetProfileAsync(HttpListenerContext context)
        {
            var user = await AuthenticateAsync(context.Request);
            if (user == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 401, "Unauthorized");
                return;
            }

            var path = context.Request.Url.AbsolutePath;
            var userIdString = path.Split("/")[3];
            if (!int.TryParse(userIdString, out var userId) || userId != user.UserId)
            {
                await HttpHelper.WriteTextAsync(context.Response, 403, "Forbidden");
                return;
            }

            var profile = new
            {
                user.UserId,
                user.Username,
                TotalRatings = _ratingManager.CountRatings(),
                AverageScore = _ratingManager.AverageRatingGiven(),
                Favorites = _favoritesManager.CountFavorites()
            };

            await HttpHelper.WriteJsonAsync(context.Response, 200, profile);
        }

        private async Task<UserAccount?> AuthenticateAsync(HttpListenerRequest request)
        {
            var authHeader = request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer"))
            {
                return null;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var userData = _jwtService.ValidateToken(token);
            if (userData == null)
            {
                return null;
            }

            return await _userRepository.FindByUsernameAsync(userData.Value.Username);
        }
    }
}
