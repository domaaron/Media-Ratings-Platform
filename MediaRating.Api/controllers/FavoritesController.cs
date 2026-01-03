using Media_Ratings_Platform.services;
using MediaRatings.Api.Utils;
using MediaRatings.Domain.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Api.controllers
{
    /*
    Manages a user's favorite media entries:
        - Adds a media entry to the user's list of favorites
        - Removes a media entry from the user's favorites
        - Validates user authentication via JWT token
        - Validates media IDs from the request path
        - Returns appropriate HTTP status codes:
            201 → successfully added
            200 → successfully removed
            400 → invalid media ID
            404 → favorite not found
            401 → unauthorized
    */
    public class FavoritesController : BaseController
    {
        private readonly IFavoritesManager _favoritesManager;

        public FavoritesController(IFavoritesManager favoritesManager, JwtService jwtService) : base(jwtService)
        {
            _favoritesManager = favoritesManager;
        }

        public async Task AddFavoriteAsync(HttpListenerContext context)
        {
            var userId = await AuthenticateAsync(context.Request, context.Response);
            if (userId == null)
            {
                return;
            }

            var mediaId = ExtractId(context.Request.Url.AbsolutePath);
            if (mediaId == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Invalid media ID");
                return;
            }

            await _favoritesManager.AddFavoriteAsync(userId.Value, mediaId.Value);
            await HttpHelper.WriteTextAsync(context.Response, 201, "Added to favorites");
        }

        public async Task RemoveFavoriteAsync(HttpListenerContext context)
        {
            var userId = await AuthenticateAsync(context.Request, context.Response);
            if (userId == null)
            {
                return;
            }

            var mediaId = ExtractId(context.Request.Url.AbsolutePath);
            if (mediaId == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Invalid media ID");
                return;
            }

            var removed = await _favoritesManager.RemoveFavoriteAsync(userId.Value, mediaId.Value);
            if (!removed)
            {
                await HttpHelper.WriteTextAsync(context.Response, 404, "Favorite not found");
            }

            await HttpHelper.WriteTextAsync(context.Response, 200, "Removed from favorites");
        }
    }
}
