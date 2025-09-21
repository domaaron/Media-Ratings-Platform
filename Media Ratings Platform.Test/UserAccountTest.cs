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
            user.AddMediaEntry(movie);

            Assert.Contains(movie, user.MediaEntries);
            Assert.Equal(1, user.TotalMediaEntries());
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
            user.AddMediaEntry(movie);
            user.RemoveMediaEntry(movie);

            Assert.DoesNotContain(movie, user.MediaEntries);
            Assert.Equal(0, user.TotalMediaEntries());
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
            user.AddFavorite(movie);

            Assert.Contains(movie, user.Favorites);
            Assert.Equal(1, user.TotalFavorites());
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
            user.AddFavorite(movie);
            user.RemoveFavorite(movie);

            Assert.DoesNotContain(movie, user.Favorites);
            Assert.Equal(0, user.TotalFavorites());
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
            user.AddRating(rating);

            Assert.Contains(user.Ratings, r => r.MediaEntry == movie);
            Assert.Equal(1, user.TotalRatings());
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
            user.AddRating(rating);
            user.RemoveRating(rating);

            Assert.DoesNotContain(user.Ratings, r => r.MediaEntry == movie);
            Assert.Equal(0, user.TotalRatings());
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
            otherUser.AddRating(rating);

            Assert.True(user.LikeRating(rating));
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
            user.AddRating(rating);

            Assert.False(user.LikeRating(rating));
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
            otherUser.AddRating(rating);

            user.LikeRating(rating);

            Assert.False(user.LikeRating(rating));
        }
    }
}