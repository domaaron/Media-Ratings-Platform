using MediaRatings.Domain;

namespace Media_Ratings_Platform.Test
{
    public class UserAccountTest
    {
        [Fact]
        public void CreateUserSuccessTest()
        {
            var user = new UserAccount("Max", "test");
            Assert.True(user != null);
        }

        [Fact]
        public void AddMediaEntryTest()
        {
            var user = new UserAccount("Max", "test");
            var movie = new Movie(
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
            var user = new UserAccount("Max", "test");
            var movie = new Movie(
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            user.MediaManager.AddMediaEntry(movie);
            user.MediaManager.RemoveMediaEntry(movie);

            Assert.DoesNotContain(movie, user.MediaManager.GetAllMediaEntries());
            Assert.Equal(0, user.MediaManager.CountMediaEntries());
        }

        [Fact]
        public void AddFavoriteSuccessTest()
        {
            var user = new UserAccount("Max", "test");
            var movie = new Movie(
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
            var user = new UserAccount("Max", "test");
            var movie = new Movie(
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
            var user = new UserAccount("Max", "test");
            var movie = new Movie(
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
            var user = new UserAccount("Max", "test");
            var movie = new Movie(
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
            var user = new UserAccount("Max", "test");
            var otherUser = new UserAccount("Alice", "test");
            var movie = new Movie(
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
            var user = new UserAccount("Max", "test");
            var movie = new Movie(
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
            var user = new UserAccount("Max", "test");
            var otherUser = new UserAccount("Alice", "test");
            var movie = new Movie(
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