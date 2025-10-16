using MediaRatings.Domain;
using MediaRatings.Domain.services;
using System;
using System.Collections.Generic;
using Xunit;

namespace Media_Ratings_Platform.Test
{
    public class UserRatingTest
    {
        [Fact]
        public void ConfirmRatingSuccessTest()
        {
            var user = new UserAccount("Max", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = new UserRating(movie, user, 5, null);
            rating.Confirm();

            Assert.True(rating.IsConfirmed);
        }

        [Fact]
        public void EditRatingSuccessTest()
        {
            var user = new UserAccount("Max", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = new UserRating(movie, user, 5, null);
            rating.EditRating(user, 4, "Nice");

            Assert.Equal(4, rating.StarValue);
            Assert.False(rating.IsConfirmed);
        }

        [Fact]
        public void EditRatingFailTest()
        {
            var user = new UserAccount("Max", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var otherUser = new UserAccount("Alice", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = new UserRating(movie, user, 5, null);

            Assert.Throws<UnauthorizedAccessException>(() => rating.EditRating(otherUser, 4, "Nice"));
        }

        [Fact]
        public void DeleteRatingSuccessTest()
        {
            var user = new UserAccount("Max", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = new UserRating(movie, user, 5, "Nice");
            rating.DeleteRating(user);

            Assert.Equal(0, rating.StarValue);
            Assert.Null(rating.Comment);
        }

        [Fact]
        public void DeleteRatingByOtherUserFailTest()
        {
            var user = new UserAccount("Max", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var otherUser = new UserAccount("Alice", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = new UserRating(movie, user, 5, "Nice");

            Assert.Throws<UnauthorizedAccessException>(() => rating.DeleteRating(otherUser));
        }

        [Fact]
        public void AddLikeSuccessTest()
        {
            var user = new UserAccount("Max", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var otherUser = new UserAccount("Alice", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = new UserRating(movie, user, 5, "Nice");
            rating.AddLike(otherUser.UserId);

            Assert.Contains(otherUser.UserId, rating.LikedBy);
        }

        [Fact]
        public void AddLikeTwiceFailTest()
        {
            var user = new UserAccount("Max", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var otherUser = new UserAccount("Alice", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = new UserRating(movie, user, 5, "Nice");
            rating.AddLike(otherUser.UserId);
            rating.AddLike(otherUser.UserId); // should not add twice

            Assert.Single(rating.LikedBy);
        }

        [Fact]
        public void RemoveLikeSuccessTest()
        {
            var user = new UserAccount("Max", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var otherUser = new UserAccount("Max", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = new UserRating(movie, user, 5, "Nice");
            rating.AddLike(otherUser.UserId);
            rating.RemoveLike(otherUser.UserId);

            Assert.DoesNotContain(otherUser.UserId, rating.LikedBy);
        }
    }
}
