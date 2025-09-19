using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Ratings_Platform.models
{
    /*
    A user:
        - registers and logs in with unique credentials (username, password)
        - can view and edit a profile with personal statistics
        - can create, update, and delete media entries
        - can rate media entries from 1–5 stars and optionally write a comment
        - can edit or delete their own ratings
        - can like other users' ratings (1 like per rating)
        - can mark media entries as favorites
        - can view their own rating history and list of favorites
        - receives recommendations based on previous rating behavior and content similarity
    */
    public class UserAccount
    {
        public UserAccount(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public Guid UserId { get; private set; } = Guid.NewGuid();
        public string Username { get; set; }
        public string Password { get; set; }

        // media created by own user
        public List<IMediaEntry> MediaEntries { get; private set; } = new List<IMediaEntry> { };
        // ratings created by own user
        public List<UserRating> Ratings { get; private set; } = new List<UserRating> { };
        // favourites, no duplicate values
        public HashSet<IMediaEntry> Favorites { get; private set; } = new HashSet<IMediaEntry>();

        /* 
         * ======================================
         * media management
         * ======================================
        */
        public void AddMediaEntry(IMediaEntry mediaEntry)
        {
            if (!MediaEntries.Contains(mediaEntry))
            {
                MediaEntries.Add(mediaEntry);
            }
        }

        public void RemoveMediaEntry(IMediaEntry mediaEntry)
        {
            MediaEntries.Remove(mediaEntry);
            Favorites.Remove(mediaEntry);
        }

        public void UpdateMediaEntry(IMediaEntry oldEntry, IMediaEntry newEntry)
        {
            int index = MediaEntries.IndexOf(oldEntry);
            if (index >= 0)
            {
                MediaEntries[index] = newEntry;
            }
        }

        /* 
         * ======================================
         * favourites
         * ======================================
        */
        public void AddFavorite(IMediaEntry mediaEntry)
        {
            Favorites.Add(mediaEntry);
        }

        public void RemoveFavorite(IMediaEntry mediaEntry)
        {
            Favorites.Remove(mediaEntry);
        }

        /* 
         * ======================================
         * ratings
         * ======================================
        */
        public UserRating AddRating(UserRating rating)
        {
            if (rating.StarValue < 1 || rating.StarValue > 5)
            {
                throw new ArgumentOutOfRangeException("Invalid rating: Stars must be between 1 and 5.");
            }

            Ratings.Add(rating);
            return rating;
        }

        public void RemoveRating(UserRating rating)
        {
            Ratings.Remove(rating);
        }

        public bool LikeRating(UserRating rating)
        {
            // cannot like own rating
            if (rating.User == this)
            {
                return false;
            }

            // cannot like twice the same rating
            if (rating.LikedBy.Contains(this.UserId))
            {
                return false;
            }

            rating.LikedBy.Add(this.UserId);
            return true;
        }

        public IEnumerable<UserRating> GetRatingHistory()
        {
            return Ratings.OrderByDescending(r => r.RatingTimestamp);
        }

        /* 
         * ======================================
         * statistics
         * ======================================
        */
        public double AverageRatingGiven()
        {
            if (!Ratings.Any())
            {
                return 0;
            }

            return Ratings.Average(r => r.StarValue);
        }

        public int TotalFavorites()
        {
            return Favorites.Count;
        }

        public int TotalMediaEntries()
        {
            return MediaEntries.Count;
        }

        public int TotalRatings()
        {
            return Ratings.Count;
        }
    }
}
