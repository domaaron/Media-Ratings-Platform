using Media_Ratings_Platform.DTOs;
using Media_Ratings_Platform.services;
using MediaRatings.Api.Utils;
using MediaRatings.Domain;
using MediaRatings.Domain.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Api.controllers
{
    public class MediaController
    {
        private readonly MediaManager _mediaManager;
        private readonly JwtService _jwtService;

        public MediaController(MediaManager mediaManager, JwtService jwtService)
        {
            _mediaManager = mediaManager;
            _jwtService = jwtService;
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
                entry.MediaId,
                entry.CreatedBy,
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
            var entries = _mediaManager.GetAllMediaEntries().OfType<MediaEntry>().Select(entry => new
            {
                entry.Title,
                entry.Description,
                MediaType = entry.MediaType.ToString().ToLower(),
                entry.ReleaseYear,
                Genres = entry.Genres.Select(g => g.ToString().ToLower()).ToList(),
                entry.AgeRestriction
            });
            await HttpHelper.WriteJsonAsync(context.Response, 200, entries);
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

        private async Task<int?> AuthenticateAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
            var authHeader = request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                await HttpHelper.WriteTextAsync(response, 401, "Unauthorized: missing token");
                return null;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var userData = _jwtService.ValidateToken(token);
            if (userData == null)
            {
                await HttpHelper.WriteTextAsync(response, 401, "Unauthorized: Invalid token");
                return null;
            }
  
            return userData.Value.UserId;
        }

        private static int? ExtractId(string path)
        {
            var parts = path.Split("/");
            return int.TryParse(parts.Last(), out var id) ? id : null;
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
