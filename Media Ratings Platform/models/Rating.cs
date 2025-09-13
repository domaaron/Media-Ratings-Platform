using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Ratings_Platform.models
{
    public class Rating
    {
        public Rating(IMediaEntry mediaEntry, UserAccount user, int starValue)
        {
            MediaEntry = mediaEntry;
            User = user;
            StarValue = starValue;
        }

        public IMediaEntry MediaEntry { get; set; }
        public UserAccount User { get; set; }
        int StarValue { get; set; }
    }
}
