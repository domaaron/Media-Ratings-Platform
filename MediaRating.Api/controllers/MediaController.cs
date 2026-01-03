using Media_Ratings_Platform.DTOs;
using Media_Ratings_Platform.services;
using MediaRatings.Api.Utils;
using MediaRatings.Domain;
using MediaRatings.Domain.interfaces;
using MediaRatings.Domain.repositories;
using MediaRatings.Domain.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Api.controllers
{
    /*
    Handles all media-related API actions:
        - Create, read, update, and delete media entries (movies, series, games)
        - Authenticate users before allowing modifications
        - Filter and search media entries by:
            - title/description
            - genre
            - type (movie, series, game)
            - release year range
        - Parse genres from string input
        - Return structured JSON responses with appropriate HTTP codes:
            201 → created
            200 → success
            204 → deleted successfully
            400 → bad request / invalid input
            403 → forbidden (user cannot modify another's entry)
            404 → not found
    */
    public class MediaController : BaseController
    {
        private readonly MediaManager _mediaManager;
        private readonly IUserRepository _userRepository;
        private readonly IRatingManager _ratingManager;

        public MediaController(MediaManager mediaManager, JwtService jwtService, IUserRepository userRepository, IRatingManager ratingManager) : base(jwtService)
        {
            _mediaManager = mediaManager;
            _userRepository = userRepository;
            _ratingManager = ratingManager;
        }

        public async Task CreateMediaAsync(HttpListenerContext context)
        {
            var userId = await AuthenticateAsync(context.Request, context.Response);
            if (userId == null)
            {
                return;
            }

            var dto = await JsonHelper.ReadBodyAsync<CreateMediaDto>(context.Request);
            if (dto == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Invalid JSON");
                return;
            }

            var genres = dto.Genres.Select(ParseGenre).ToList();
            var mediaType = dto.MediaType?.Trim().ToLower();

            MediaEntry entry = mediaType switch
            {
                "movie" => new Movie(userId.Value, dto.Title, dto.Description, dto.ReleaseYear, genres, dto.AgeRestriction),
                "series" => new Series(userId.Value, dto.Title, dto.Description, dto.ReleaseYear, genres, dto.AgeRestriction),
                "game" => new Game(userId.Value, dto.Title, dto.Description, dto.ReleaseYear, genres, dto.AgeRestriction),
                _ => throw new InvalidOperationException("Unknown media type")
            };

            _mediaManager.AddMediaEntry(entry);

            var result = new
            {
                entry.Title,
                entry.Description,
                MediaType = entry.MediaType.ToString().ToLower(),
                entry.ReleaseYear,
                Genres = entry.Genres.Select(g => g.ToString().ToLower()).ToList(),
                entry.AgeRestriction,
                entry.Ratings,
                entry.FavoritedBy
            };

            await HttpHelper.WriteJsonAsync(context.Response, 201, result);
        }

        public async Task GetAllMediaAsync(HttpListenerContext context)
        {
            var query = context.Request.QueryString;

            var search = query["title"] ?? query["search"];
            var genre = query["genre"];
            var mediaType = query["mediaType"] ?? query["type"];
            var minYear = int.TryParse(query["releaseYearMin"], out var minY) ? minY : (int?)null;
            var maxYear = int.TryParse(query["releaseYearMax"], out var maxY) ? maxY : (int?)null;

            var entries = _mediaManager.GetAllMediaEntries().OfType<MediaEntry>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                entries = entries.Where(m =>
                    m.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(m.Description) && m.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }

            if (!string.IsNullOrWhiteSpace(mediaType))
            {
                entries = entries.Where(m =>
                    m.MediaType.ToString().Equals(mediaType, StringComparison.OrdinalIgnoreCase)
                );
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                entries = entries.Where(m =>
                    m.Genres.Any(g => g.ToString().ToLower() == genre.ToLower().Replace("-", "")) // SciFi == scifi
                );
            }

            if (minYear.HasValue)
            {
                entries = entries.Where(m => m.ReleaseYear >= minYear.Value);
            }

            if (maxYear.HasValue)
            {
                entries = entries.Where(m => m.ReleaseYear <= maxYear.Value);
            }

            var result = entries.Select(entry => new
            {
                entry.MediaId,
                entry.Title,
                entry.Description,
                MediaType = entry.MediaType.ToString().ToLower(),
                entry.ReleaseYear,
                Genres = entry.Genres.Select(g => g.ToString().ToLower()).ToList(),
                entry.AgeRestriction
            });

            await HttpHelper.WriteJsonAsync(context.Response, 200, result);
        }

        public async Task GetMediaByIdAsync(HttpListenerContext context)
        {
            var id = ExtractId(context.Request.Url.AbsolutePath);
            if (id == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Invalid ID");
                return;
            }

            var entry = _mediaManager.GetMediaById(id.Value);
            if (entry == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 404, "Not found");
                return;
            }

            var result = new
            {
                entry.MediaId,
                entry.CreatedBy,
                entry.Title,
                entry.Description,
                MediaType = entry.MediaType.ToString().ToLower(),
                entry.ReleaseYear,
                Genres = entry.Genres.Select(g => g.ToString().ToLower()).ToList(),
                entry.AgeRestriction
            };

            await HttpHelper.WriteJsonAsync(context.Response, 200, result);
        }

        public async Task UpdateMediaAsync(HttpListenerContext context)
        {
            var userId = await AuthenticateAsync(context.Request, context.Response);
            if (userId == null)
            {
                return;
            }

            var id = ExtractId(context.Request.Url.AbsolutePath);
            var dto = await JsonHelper.ReadBodyAsync<UpdateMediaDto>(context.Request);
            if (id == null || dto == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Invalid data");
                return;
            }

            var entry = _mediaManager.GetMediaById(id.Value);
            if (entry == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 404, "Not found");
                return;
            }

            if (entry.CreatedBy != userId)
            {
                await HttpHelper.WriteTextAsync(context.Response, 403, "Forbidden: Not your media entry");
                return;
            }

            var updated = _mediaManager.UpdateMediaEntry(id.Value, dto);
            if (!updated)
            {
                await HttpHelper.WriteTextAsync(context.Response, 404, "Not found");
                return;
            }

            await HttpHelper.WriteTextAsync(context.Response, 200, "Updated");
        }

        public async Task DeleteMediaAsync(HttpListenerContext context)
        {
            var userId = await AuthenticateAsync(context.Request, context.Response);
            if (userId == null)
            {
                return;
            }

            var id = ExtractId(context.Request.Url.AbsolutePath);
            if (id == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Invalid data");
                return;
            }

            var entry = _mediaManager.GetMediaById(id.Value);
            if (entry == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 404, "Not found");
                return;
            }

            if (entry.CreatedBy != userId)
            {
                await HttpHelper.WriteTextAsync(context.Response, 403, "Forbidden: Not your media entry");
                return;
            }

            var deleted = _mediaManager.RemoveMediaEntry(id.Value);
            await HttpHelper.WriteTextAsync(context.Response, deleted ? 204 : 404, deleted ? "" : "Not found");
        }

        private static Genres ParseGenre(string genre) => genre.ToLower() switch
        {
            "action" => Genres.Action,
            "thriller" => Genres.Thriller,
            "sci-fi" => Genres.SciFi,
            "animation" => Genres.Animation,
            "comedy" => Genres.Comedy,
            "drama" => Genres.Drama,
            "fantasy" => Genres.Fantasy,
            "adventure" => Genres.Adventure,
            "unknown" => Genres.Unknown
        };
    }
}
