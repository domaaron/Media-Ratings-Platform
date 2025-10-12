using MediaRatings.Domain.interfaces;
using MediaRatings.Domain.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain
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
        private static int _nextId = 1;
        public UserAccount(string username, string password, IMediaManager mediaManager, IFavoritesManager favoritesManager, IRatingManager ratingManager)
        {
            UserId = _nextId++;
            Username = username;
            Password = password;
            MediaManager = mediaManager;
            FavoritesManager = favoritesManager;
            RatingManager = ratingManager;
        }

        public int UserId { get; private set; }
        public string Username { get; set; }
        public string Password { get; set; }

        // services
        public IMediaManager MediaManager { get; private set; }
        public IFavoritesManager FavoritesManager { get; private set; }
        public IRatingManager RatingManager { get; private set; }
    }
}
