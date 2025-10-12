using MediaRatings.Domain;
using MediaRatings.Domain.services;

namespace Media_Ratings_Platform.Test
{
    public class UserAccountTest
    {
        private (UserAccount user, MediaManager mediaManager, FavoritesManager favoritesManager, RatingManager ratingManager) CreateUserWithServices(string username)
        {
            var mediaManager = new MediaManager();
            var favoritesManager = new FavoritesManager();
            var ratingManager = new RatingManager(null!);
            var user = new UserAccount(username, "test", mediaManager, favoritesManager, ratingManager);

            // Owner für RatingManager setzen
            typeof(RatingManager).GetField("_owner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(ratingManager, user);

            return (user, mediaManager, favoritesManager, ratingManager);
        }

        [Fact]
        public void CreateUserSuccessTest()
        {
            var (user, _, _, _) = CreateUserWithServices("Max");
            Assert.NotNull(user);
        }

        [Fact]
        public void AddMediaEntryTest()
        {
            var (user, mediaManager, _, _) = CreateUserWithServices("Max");

            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );

            user.MediaManager.AddMediaEntry(movie);

            Assert.Contains(movie, user.MediaManager.GetAllMediaEntries());
            Assert.Equal(1, user.MediaManager.CountMediaEntries());
        }

        [Fact]
        public void RemoveMediaEntrySuccessTest()
        {
            var (user, mediaManager, _, _) = CreateUserWithServices("Max");

            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );

            user.MediaManager.AddMediaEntry(movie);
            user.MediaManager.RemoveMediaEntry(movie.MediaId);

            Assert.DoesNotContain(movie, user.MediaManager.GetAllMediaEntries());
            Assert.Equal(0, user.MediaManager.CountMediaEntries());
        }

        [Fact]
        public void AddFavoriteSuccessTest()
        {
            var (user, _, favoritesManager, _) = CreateUserWithServices("Max");

            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );

            user.FavoritesManager.AddFavorite(movie);

            Assert.Contains(movie, user.FavoritesManager.GetAllFavorites());
            Assert.Equal(1, user.FavoritesManager.CountFavorites());
        }

        [Fact]
        public void RemoveFavoriteSuccessTest()
        {
            var (user, _, favoritesManager, _) = CreateUserWithServices("Max");

            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );

            user.FavoritesManager.AddFavorite(movie);
            user.FavoritesManager.RemoveFavorite(movie);

            Assert.DoesNotContain(movie, user.FavoritesManager.GetAllFavorites());
            Assert.Equal(0, user.FavoritesManager.CountFavorites());
        }

        [Fact]
        public void AddRatingSuccessTest()
        {
            var (user, _, _, ratingManager) = CreateUserWithServices("Max");

            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );

            var rating = new UserRating(movie, user, 5, "Can recommend");
            user.RatingManager.AddRating(rating);

            Assert.Contains(user.RatingManager.GetAllRatings(), r => r.MediaEntry == movie);
            Assert.Equal(1, user.RatingManager.CountRatings());
        }

        [Fact]
        public void RemoveRatingSuccessTest()
        {
            var (user, _, _, ratingManager) = CreateUserWithServices("Max");

            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );

            var rating = new UserRating(movie, user, 5, "Can recommend");
            user.RatingManager.AddRating(rating);
            user.RatingManager.RemoveRating(rating);

            Assert.DoesNotContain(user.RatingManager.GetAllRatings(), r => r.MediaEntry == movie);
            Assert.Equal(0, user.RatingManager.CountRatings());
        }

        [Fact]
        public void LikeRatingSuccessTest()
        {
            var (user, _, _, ratingManager) = CreateUserWithServices("Max");
            var (otherUser, _, _, otherRatingManager) = CreateUserWithServices("Alice");

            var movie = new Movie(
                otherUser.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );

            var rating = new UserRating(movie, otherUser, 5, "Can recommend");
            otherUser.RatingManager.AddRating(rating);

            Assert.True(user.RatingManager.LikeRating(rating, user));
        }

        [Fact]
        public void LikeOwnRatingFailTest()
        {
            var (user, _, _, ratingManager) = CreateUserWithServices("Max");

            var movie = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );

            var rating = new UserRating(movie, user, 5, "Can recommend");
            user.RatingManager.AddRating(rating);

            Assert.False(user.RatingManager.LikeRating(rating, user));
        }

        [Fact]
        public void LikeSameRatingTwiceFailTest()
        {
            var (user, _, _, ratingManager) = CreateUserWithServices("Max");
            var (otherUser, _, _, otherRatingManager) = CreateUserWithServices("Alice");

            var movie = new Movie(
                otherUser.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );

            var rating = new UserRating(movie, otherUser, 5, "Can recommend");
            otherUser.RatingManager.AddRating(rating);

            user.RatingManager.LikeRating(rating, user);

            Assert.False(user.RatingManager.LikeRating(rating, user));
        }
    }
}
