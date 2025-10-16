using MediaRatings.Domain;
using MediaRatings.Domain.services;

namespace Media_Ratings_Platform.Test
{
    public class UserAccountTest
    {
        [Fact]
        public void CreateUserSuccessTest()
        {
            var user = new UserAccount("Max", "test", new MediaManager(), new FavoritesManager(), new RatingManager());

            Assert.NotNull(user);
        }

        [Fact]
        public void AddMediaEntryTest()
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

            user.MediaManager.AddMediaEntry(movie);

            Assert.Contains(movie, user.MediaManager.GetAllMediaEntries());
            Assert.Equal(1, user.MediaManager.CountMediaEntries());
        }

        [Fact]
        public void RemoveMediaEntrySuccessTest()
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

            user.MediaManager.AddMediaEntry(movie);
            user.MediaManager.RemoveMediaEntry(movie.MediaId);

            Assert.DoesNotContain(movie, user.MediaManager.GetAllMediaEntries());
            Assert.Equal(0, user.MediaManager.CountMediaEntries());
        }

        [Fact]
        public void AddFavoriteSuccessTest()
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

            user.FavoritesManager.AddFavorite(movie);

            Assert.Contains(movie, user.FavoritesManager.GetAllFavorites());
            Assert.Equal(1, user.FavoritesManager.CountFavorites());
        }

        [Fact]
        public void AddSameFavoriteTwiceFailTest()
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

            user.FavoritesManager.AddFavorite(movie);
            user.FavoritesManager.AddFavorite(movie);

            Assert.Single(user.FavoritesManager.GetAllFavorites());
        }

        [Fact]
        public void RemoveFavoriteSuccessTest()
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

            user.FavoritesManager.AddFavorite(movie);
            user.FavoritesManager.RemoveFavorite(movie);

            Assert.DoesNotContain(movie, user.FavoritesManager.GetAllFavorites());
            Assert.Equal(0, user.FavoritesManager.CountFavorites());
        }

        [Fact]
        public void AddRatingSuccessTest()
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

            var rating = new UserRating(movie, user, 5, "Can recommend");
            user.RatingManager.AddRating(rating);

            Assert.Contains(user.RatingManager.GetAllRatings(), r => r.MediaEntry == movie);
            Assert.Equal(1, user.RatingManager.CountRatings());
        }

        [Fact]
        public void RemoveRatingSuccessTest()
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

            var rating = new UserRating(movie, user, 5, "Can recommend");
            user.RatingManager.AddRating(rating);
            user.RatingManager.RemoveRating(rating);

            Assert.DoesNotContain(user.RatingManager.GetAllRatings(), r => r.MediaEntry == movie);
            Assert.Equal(0, user.RatingManager.CountRatings());
        }

        [Fact]
        public void LikeRatingSuccessTest()
        {
            var user = new UserAccount("Max", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var otherUser = new UserAccount("Alice", "test", new MediaManager(), new FavoritesManager(), new RatingManager());

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
            var user = new UserAccount("Max", "test", new MediaManager(), new FavoritesManager(), new RatingManager());

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
            var user = new UserAccount("Max", "test", new MediaManager(), new FavoritesManager(), new RatingManager());
            var otherUser = new UserAccount("Alice", "test", new MediaManager(), new FavoritesManager(), new RatingManager());

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

        [Fact]
        public void GetMediaByIdSuccessTest()
        {
            var user = new UserAccount("Max", "test", new MediaManager(), new FavoritesManager(), new RatingManager());

            var movie1 = new Movie(
                user.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );

            var movie2 = new Movie(
                user.UserId,
                "Inception",
                "Dreams",
                2006,
                new List<Genres> { Genres.SciFi, Genres.Action },
                6
            );

            user.MediaManager.AddMediaEntry(movie1);
            user.MediaManager.AddMediaEntry(movie2);

            var foundMovie = user.MediaManager.GetMediaById(movie1.MediaId);
            Assert.NotNull(foundMovie);
            Assert.Equal(movie1, foundMovie);
        }
    }
}
