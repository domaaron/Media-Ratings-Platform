
using MediaRatings.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Ratings_Platform.Test
{
    public class UserRatingTest
    {
        [Fact]
        public void ConfirmRatingSuccessTest()
        {
            var user = new UserAccount("Max", "test");
            var movie = new Movie(
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
            var user = new UserAccount("Max", "test");
            var movie = new Movie(
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
            var user = new UserAccount("Max", "test");
            var otherUser = new UserAccount("Alice", "test");
            var movie = new Movie(
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
            var user = new UserAccount("Max", "test");
            var otherUser = new UserAccount("Alice", "test");
            var movie = new Movie(
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = new UserRating(movie, user, 5, "Nice");
            rating.DeleteRating();

            Assert.Equal(0, rating.StarValue);
            Assert.Null(rating.Comment);
        }

        [Fact]
        public void AddLikeSuccessTest()
        {
            var user = new UserAccount("Max", "test");
            var otherUserId = Guid.NewGuid();
            var movie = new Movie(
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = new UserRating(movie, user, 5, "Nice");
            rating.AddLike(otherUserId);

            Assert.Contains(otherUserId, rating.LikedBy);
        }

        [Fact]
        public void AddLikeTwiceFailTest()
        {
            var user = new UserAccount("Max", "test");
            var otherUserId = Guid.NewGuid();
            var movie = new Movie(
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = new UserRating(movie, user, 5, "Nice");
            rating.AddLike(otherUserId);
            rating.AddLike(otherUserId);

            Assert.Single(rating.LikedBy);
        }

        [Fact]
        public void RemoveLikeSuccessTest()
        {
            var user = new UserAccount("Max", "test");
            var otherUserId = Guid.NewGuid();
            var movie = new Movie(
                "Cars",
                "It's about cars.",
                2006,
                new List<Genres> { Genres.Animation, Genres.Comedy },
                6
            );
            var rating = new UserRating(movie, user, 5, "Nice");
            rating.AddLike(otherUserId);
            rating.RemoveLike(otherUserId);

            Assert.DoesNotContain(otherUserId, rating.LikedBy);
        }
    }
}
