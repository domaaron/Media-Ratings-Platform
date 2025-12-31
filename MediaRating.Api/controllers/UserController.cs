using Media_Ratings_Platform.DTOs;
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

            var totalRatings = await _ratingManager.CountRatingsAsync(user.UserId);
            var averageScore = await _ratingManager.AverageRatingGivenAsync(user.UserId);
            var favoritesCount = await _favoritesManager.CountFavoritesAsync(user.UserId);

            var profile = new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.FavoriteGenre,
                TotalRatings = totalRatings,
                AverageScore = averageScore,
                Favorites = favoritesCount
            };

            await HttpHelper.WriteJsonAsync(context.Response, 200, profile);
        }

        public async Task GetRatingHistoryAsync(HttpListenerContext context)
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

            var ratings = await _ratingManager.GetRatingHistoryAsync(userId);

            var result = ratings.Select(r => new
            {
                RatingId = r.RatingId,
                MediaId = r.MediaEntry.MediaId,
                MediaTitle = r.MediaEntry.Title,
                Stars = r.StarValue,
                Comment = r.IsConfirmed ? r.Comment : null,
                CreatedAt = r.RatingTimestamp,
                IsConfirmed = r.IsConfirmed
            });

            await HttpHelper.WriteJsonAsync(context.Response, 200, result);

        }

        public async Task GetFavoritesAsync(HttpListenerContext context)
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

            var favorites = await _favoritesManager.GetAllFavoritesAsync(user.UserId);

            var result = favorites.Select(f => new
            {
                MediaId = f.MediaId,
                Title = f.Title,
                MediaType = f.MediaType,
                ReleaseYear = f.ReleaseYear
            });

            await HttpHelper.WriteJsonAsync(context.Response, 200, result);
        }

        public async Task UpdateProfileAsync(HttpListenerContext context)
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

            var dto = await JsonHelper.ReadBodyAsync<UpdateProfileDto>(context.Request);
            if (dto == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Invalid profile data");
                return;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                user.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.FavoriteGenre))
            {
                user.FavoriteGenre = dto.FavoriteGenre;
            }

            await _userRepository.UpdateProfileAsync(user);

            await HttpHelper.WriteJsonAsync(context.Response, 200, new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.FavoriteGenre
            });
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
