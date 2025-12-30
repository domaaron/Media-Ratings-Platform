using Media_Ratings_Platform.DTOs;
using Media_Ratings_Platform.services;
using MediaRatings.Api.Utils;
using MediaRatings.Domain;
using MediaRatings.Domain.interfaces;
using MediaRatings.Domain.repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Api.controllers
{
    public class RatingController : BaseController
    {
        private readonly IRatingManager _ratingManager;
        private readonly IMediaManager _mediaManager;
        private readonly IUserRepository _userRepository;

        public RatingController(IRatingManager ratingManager, IMediaManager mediaManager, IUserRepository userRepository, JwtService jwtService) : base(jwtService)
        {
            _ratingManager = ratingManager;
            _mediaManager = mediaManager;
            _userRepository = userRepository;
        }

        public async Task RateMediaAsync(HttpListenerContext context)
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

            var dto = await JsonHelper.ReadBodyAsync<RateMediaDto>(context.Request);
            if (dto == null || dto.Stars < 1 || dto.Stars > 5)
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Invalid rating data");
                return;
            }

            var mediaEntry = _mediaManager.GetMediaById(mediaId.Value);
            if (mediaEntry == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 404, "Media not found");
                return;
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null) 
            {
                await HttpHelper.WriteTextAsync(context.Response, 404, "User not found");
                return;
            }

            var rating = UserRating.Create(mediaEntry, user, dto.Stars, dto.Comment);

            await _ratingManager.AddRatingAsync(rating);
            await HttpHelper.WriteJsonAsync(context.Response, 201, new
            {
                mediaId = mediaEntry.MediaId,
                stars = dto.Stars,
                comment = dto.Comment
            });
        }

        public async Task EditRatingAsync(HttpListenerContext context)
        {
            var userId = await AuthenticateAsync(context.Request, context.Response);
            if (userId == null)
            {
                return;
            }

            var segments = context.Request.Url.AbsolutePath.Trim('/').Split('/');
            if (segments.Length < 3 || !int.TryParse(segments[2], out var ratingId))
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Invalid rating ID");
                return;
            }

            var dto = await JsonHelper.ReadBodyAsync<EditRatingDto>(context.Request);
            if (dto == null || dto.Stars < 1 || dto.Stars > 5)
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Invalid rating data");
                return;
            }

            var rating = await _ratingManager.GetRatingByIdAsync(ratingId);
            if (rating == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 404, "Rating not found");
                return;
            }

            if (rating.User == null || rating.User.UserId != userId.Value)
            {
                await HttpHelper.WriteTextAsync(context.Response, 403, "You can only edit your own ratings.");
                return;
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            rating.EditRating(user, dto.Stars, dto.Comment);
            await _ratingManager.UpdateRatingAsync(rating);

            await HttpHelper.WriteJsonAsync(context.Response, 200, new { rating.RatingId, rating.StarValue, rating.Comment });
        }

        public async Task LikeRatingAsync(HttpListenerContext context)
        {
            var userId = await AuthenticateAsync(context.Request, context.Response);
            if (userId == null) return;

            var segments = context.Request.Url.AbsolutePath.Trim('/').Split('/');
            if (segments.Length < 3 || !int.TryParse(segments[2], out var ratingId))
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Invalid rating ID");
                return;
            }

            var rating = await _ratingManager.GetRatingByIdAsync(ratingId);
            if (rating == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 404, "Rating not found");
                return;
            }

            var likingUser = await _userRepository.GetByIdAsync(userId.Value);
            var success = await _ratingManager.LikeRatingAsync(rating, likingUser);
            if (!success)
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Cannot like rating (maybe your own or already liked)");
                return;
            }

            await HttpHelper.WriteTextAsync(context.Response, 200, "Rating liked");
        }

        public async Task ConfirmRatingAsync(HttpListenerContext context)
        {
            var userId = await AuthenticateAsync(context.Request, context.Response);
            if (userId == null)
            {
                return;
            }

            var segments = context.Request.Url.AbsolutePath.Trim('/').Split('/');
            if (segments.Length < 3 || !int.TryParse(segments[2], out var ratingId))
            {
                await HttpHelper.WriteTextAsync(context.Response, 400, "Invalid rating ID");
                return;
            }

            var rating = await _ratingManager.GetRatingByIdAsync(ratingId);
            if (rating == null)
            {
                await HttpHelper.WriteTextAsync(context.Response, 404, "Rating not found");
                return;
            }

            // only creator of rating can confirm
            if (rating.User.UserId != userId.Value)
            {
                await HttpHelper.WriteTextAsync(context.Response, 403, "Cannot confirm another user's rating");
                return;
            }

            rating.Confirm();
            await _ratingManager.UpdateRatingAsync(rating);

            await HttpHelper.WriteTextAsync(context.Response, 200, "Rating confirmed");
        }
    }
}
