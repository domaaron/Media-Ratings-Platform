using Media_Ratings_Platform.Test.fakes;
using MediaRatings.Domain;
using MediaRatings.Domain.services;
using System;
using System.Collections.Generic;
using Xunit;

namespace Media_Ratings_Platform.Test.unit
{
    public class UserRatingTest
    {
        private static int _nextUserId = 1;

        private UserAccount CreateTestUser(string username)
        {
            var mediaManager = new InMemoryMediaManager();
            var favoritesManager = new InMemoryFavoritesManager();
            var ratingManager = new InMemoryRatingManager();

            var user = new UserAccount(username, "password", mediaManager, favoritesManager, ratingManager);
            typeof(UserAccount).GetProperty("UserId")!.SetValue(user, _nextUserId++);
            return user;
        }

        [Fact]
        public void ConfirmRatingSuccessTest()
        {
            var user = CreateTestUser("Max");
            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = UserRating.Create(movie, user, 5, null, 1);
            rating.Confirm();

            Assert.True(rating.IsConfirmed);
        }

        [Fact]
        public void EditRatingSuccessTest()
        {
            var user = CreateTestUser("Max");
            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = UserRating.Create(movie, user, 5, null, 1);
            rating.EditRating(user, 4, "Nice");

            Assert.Equal(4, rating.StarValue);
            Assert.Equal("Nice", rating.Comment);
            Assert.False(rating.IsConfirmed);
        }

        [Fact]
        public void EditRatingFailTest()
        {
            var owner = CreateTestUser("Max");
            var otherUser = CreateTestUser("Alice");
            var movie = new Movie(
                owner.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = UserRating.Create(movie, owner, 5, null, 1);

            Assert.Throws<UnauthorizedAccessException>(() => rating.EditRating(otherUser, 4, "Nice"));
        }

        [Fact]
        public void DeleteRatingSuccessTest()
        {
            var user = CreateTestUser("Max");
            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = UserRating.Create(movie, user, 5, "Nice", 1);
            rating.DeleteRating(user);

            Assert.Equal(0, rating.StarValue);
            Assert.Null(rating.Comment);
        }

        [Fact]
        public void DeleteRatingByOtherUserFailTest()
        {
            var owner = CreateTestUser("Max");
            var otherUser = CreateTestUser("Alice");
            var movie = new Movie(
                owner.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = UserRating.Create(movie, owner, 5, "Nice", 1);

            Assert.Throws<UnauthorizedAccessException>(() => rating.DeleteRating(otherUser));
        }

        [Fact]
        public void AddLikeSuccessTest()
        {
            var owner = CreateTestUser("Max");
            var otherUser = CreateTestUser("Alice");
            var movie = new Movie(
                owner.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = UserRating.Create(movie, owner, 5, "Nice", 1);
            rating.AddLike(otherUser.UserId);

            Assert.Contains(otherUser.UserId, rating.LikedBy);
        }

        [Fact]
        public void AddLikeTwiceFailTest()
        {
            var owner = CreateTestUser("Max");
            var otherUser = CreateTestUser("Alice");
            var movie = new Movie(
                owner.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = UserRating.Create(movie, owner, 5, "Nice", 1);
            rating.AddLike(otherUser.UserId);
            rating.AddLike(otherUser.UserId); // should not add twice

            Assert.Single(rating.LikedBy);
        }

        [Fact]
        public void RemoveLikeSuccessTest()
        {
            var owner = CreateTestUser("Max");
            var otherUser = CreateTestUser("Max");
            var movie = new Movie(
                owner.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = UserRating.Create(movie, owner, 5, "Nice", 1);
            rating.AddLike(otherUser.UserId);
            rating.RemoveLike(otherUser.UserId);

            Assert.DoesNotContain(otherUser.UserId, rating.LikedBy);
        }
    }
}
