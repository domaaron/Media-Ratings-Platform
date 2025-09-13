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
        public List<UserRating> Rating { get; private set; } = new List<UserRating> { };

        // no duplicate values
        public HashSet<IMediaEntry> Favorites { get; private set; } = new HashSet<IMediaEntry>();
        public void AddFavorite(IMediaEntry mediaEntry)
        {
            Favorites.Add(mediaEntry);
        }

        public void RemoveFavorite(IMediaEntry mediaEntry)
        {
            Favorites.Remove(mediaEntry);
        }

        public void AddMediaEntry(IMediaEntry mediaEntry)
        {
            MediaEntries.Add(mediaEntry);
        }

        public void RemoveMediaEntry(IMediaEntry mediaEntry)
        {
            MediaEntries.Remove(mediaEntry);
        }

        public void AddRating(UserRating rating)
        {
            Rating.Add(rating);
        }

        public void RemoveRating(UserRating rating)
        {
            Rating.Remove(rating);
        }

        public int TotalFavorites()
        {
            return MediaEntries.Count;
        }

        public int TotalMediaEntries()
        {
            return MediaEntries.Count;
        }
    }
}
