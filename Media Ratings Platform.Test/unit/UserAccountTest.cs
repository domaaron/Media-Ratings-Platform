using Media_Ratings_Platform.Test.fakes;
using MediaRatings.Domain;
using MediaRatings.Domain.services;

namespace Media_Ratings_Platform.Test.unit
{
    public class UserAccountTest
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
        public void CreateUserSuccessTest()
        {
            var user = CreateTestUser("Max");

            Assert.NotNull(user);
        }

        [Fact]
        public void AddMediaEntryTest()
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

            user.MediaManager.AddMediaEntry(movie);

            Assert.Contains(movie, user.MediaManager.GetAllMediaEntries());
            Assert.Equal(1, user.MediaManager.CountMediaEntries());
        }

        [Fact]
        public void RemoveMediaEntrySuccessTest()
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

            user.MediaManager.AddMediaEntry(movie);
            user.MediaManager.RemoveMediaEntry(movie.MediaId);

            Assert.DoesNotContain(movie, user.MediaManager.GetAllMediaEntries());
            Assert.Equal(0, user.MediaManager.CountMediaEntries());
        }

        [Fact]
        public async Task AddFavoriteSuccessTest()
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

            user.MediaManager.AddMediaEntry(movie);
            await user.FavoritesManager.AddFavoriteAsync(user.UserId, movie.MediaId);

            var favorites = await user.FavoritesManager.GetAllFavoritesAsync(user.UserId);
            var count = await user.FavoritesManager.CountFavoritesAsync(user.UserId);

            Assert.Single(favorites);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task AddSameFavoriteTwiceFailTest()
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

            user.MediaManager.AddMediaEntry(movie);
            await user.FavoritesManager.AddFavoriteAsync(user.UserId, movie.MediaId);
            await user.FavoritesManager.AddFavoriteAsync(user.UserId, movie.MediaId);

            var favorites = await user.FavoritesManager.GetAllFavoritesAsync(user.UserId);

            Assert.Single(favorites);
        }

        [Fact]
        public async Task RemoveFavoriteSuccessTest()
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

            user.MediaManager.AddMediaEntry(movie);
            await user.FavoritesManager.AddFavoriteAsync(user.UserId, movie.MediaId);
            await user.FavoritesManager.RemoveFavoriteAsync(user.UserId, movie.MediaId);

            var favorites = await user.FavoritesManager.GetAllFavoritesAsync(user.UserId);
            Assert.Empty(favorites);
        }

        [Fact]
        public async Task AddRatingSuccessTest()
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

            var rating = UserRating.Create(movie, user, 5, "Can recommend", 1);
            await user.RatingManager.AddRatingAsync(rating);
            var ratings = await user.RatingManager.GetAllRatingsAsync(user.UserId);

            Assert.Single(ratings);
        }

        [Fact]
        public async Task RemoveRatingSuccessTest()
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

            var rating = UserRating.Create(movie, user, 5, "Can recommend", 1);
            await user.RatingManager.AddRatingAsync(rating);
            await user.RatingManager.RemoveRatingAsync(rating);

            var ratings = await user.RatingManager.GetAllRatingsAsync(user.UserId);

            Assert.Empty(ratings);
        }

        [Fact]
        public async Task LikeRatingSuccessTest()
        {
            //var ratingManager = new InMemoryRatingManager();

            var user = CreateTestUser("Max");
            //typeof(UserAccount).GetProperty("UserId")!.SetValue(user, 1);

            var otherUser = CreateTestUser("Alice");
            //typeof(UserAccount).GetProperty("UserId")!.SetValue(otherUser, 2);


            var movie = new Movie(
                otherUser.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );

            var rating = UserRating.Create(movie, otherUser, 5, "Can recommend", 0); // Id=0, Manager setzt eigene Id
            await otherUser.RatingManager.AddRatingAsync(rating);

            var result = await user.RatingManager.LikeRatingAsync(rating, user);

            Assert.True(result);


        }


        [Fact]
        public async Task LikeOwnRatingFailTest()
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

            var rating = UserRating.Create(movie, user, 5, "Can recommend", 1);
            await user.RatingManager.AddRatingAsync(rating);
            var result = await user.RatingManager.LikeRatingAsync(rating, user);

            Assert.False(result);
        }

        [Fact]
        public async Task LikeSameRatingTwiceFailTest()
        {
            var user = CreateTestUser("Max");
            var otherUser = CreateTestUser("Alice");

            var movie = new Movie(
                otherUser.UserId,
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );

            var rating = UserRating.Create(movie, otherUser, 5, "Can recommend", 1);
            await otherUser.RatingManager.AddRatingAsync(rating);

            var firstLike = await user.RatingManager.LikeRatingAsync(rating, user);
            var secondLike = await user.RatingManager.LikeRatingAsync(rating, user);

            Assert.False(secondLike);
        }

        [Fact]
        public async Task GetMediaByIdSuccessTest()
        {
            var user = CreateTestUser("Max");

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
