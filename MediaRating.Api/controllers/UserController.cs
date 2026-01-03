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
    /*
    Handles all user-related API actions:
        - Authenticates users via JWT tokens
        - Provides user profile information and statistics
        - Allows users to update their profile (email, favorite genre)
        - Retrieves user's rating history
        - Retrieves user's list of favorite media
        - Provides personalized media recommendations based on:
            - favorite genre
            - previously rated media (content similarity)
        - Validates input and authorization, returning appropriate HTTP codes:
            200 → success
            201 → created
            400 → bad request / invalid input
            401 → unauthorized (invalid or missing token)
            403 → forbidden (attempting actions for another user)
    */
    public class UserController
    {
        private readonly JwtService _jwtService;
        private readonly UserRepository _userRepository;
        private readonly RatingManager _ratingManager;
        private readonly FavoritesManager _favoritesManager;
        private readonly MediaManager _mediaManager;

        public UserController(JwtService jwtService, UserRepository userRepository, RatingManager ratingManager, FavoritesManager favoritesManager, MediaManager mediaManager)
        {
            _jwtService = jwtService;
            _userRepository = userRepository;
            _ratingManager = ratingManager;
            _favoritesManager = favoritesManager;
            _mediaManager = mediaManager;
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

        public async Task GetRecommendationsAsync(HttpListenerContext context)
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

            var query = context.Request.QueryString["type"];

            IEnumerable<IMediaEntry> recommendations = query switch
            {
                "genre" => await GetGenreRecommendations(user),
                "content" => await GetContentRecommendations(user),
                _ => Enumerable.Empty<IMediaEntry>()
            };

            var result = recommendations.Select(m => new
            {
                m.MediaId,
                m.Title,
                m.MediaType,
                m.ReleaseYear,
                Genres = m.Genres.Select(g => g.ToString())
            });

            await HttpHelper.WriteJsonAsync(context.Response, 200, result);
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

        private async Task<IEnumerable<IMediaEntry>> GetGenreRecommendations(UserAccount user)
        {
            if (string.IsNullOrWhiteSpace(user.FavoriteGenre))
            {
                return Enumerable.Empty<IMediaEntry>();
            }

            var allMedia = _mediaManager.GetAllMediaEntries();

            return allMedia
                .Where(m => m.Genres.Any(g =>
                    g.ToString().Equals(user.FavoriteGenre, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(m => m.AverageRating())
                .Take(5);
        }

        private async Task<IEnumerable<IMediaEntry>> GetContentRecommendations(UserAccount user)
        {
            var ratedMedia = await _ratingManager.GetRatedMediaAsync(user.UserId);

            if (!ratedMedia.Any())
            {
                return Enumerable.Empty<IMediaEntry>();
            }

            // preferred genres from previous reviews
            var preferredGenres = ratedMedia
                .SelectMany(m => m.Genres)
                .GroupBy(g => g)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToList();

            var allMedia = _mediaManager.GetAllMediaEntries();

            return allMedia
                .Where(m =>
                    m.Genres.Any(g => preferredGenres.Contains(g)) &&
                    !ratedMedia.Any(r => r.MediaId == m.MediaId))
                .OrderByDescending(m => m.AverageRating())
                .Take(5);
        }
    }
}
