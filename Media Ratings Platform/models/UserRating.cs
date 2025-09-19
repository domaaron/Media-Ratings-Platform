using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Ratings_Platform.models
{
    /* 
    A rating:
        - is tied to a specific media entry and a specific user
        - contains: star value (1–5), optional comment, timestamp
        - can be liked by other users
        - can be edited or deleted by the user who created it
        - requires confirmation by the creator before the comment becomes publicly visible (moderation feature)
            - comments are not publicly visible until confirmed by the author
    */
    public class UserRating
    {
        public UserRating(IMediaEntry mediaEntry, UserAccount user, int starValue, string? comment)
        {
            MediaEntry = mediaEntry;
            User = user;
            StarValue = starValue;
            Comment = comment;
            RatingTimestamp = DateTime.Now;
        }

        public Guid RatingId { get; private set; } = Guid.NewGuid();
        public IMediaEntry MediaEntry { get; private set; }
        public UserAccount User { get; private set; }
        public int StarValue { get; private set; }
        public string? Comment { get; private set; }
        public DateTime RatingTimestamp { get; private set; }
        public bool IsConfirmed { get; private set; } = false;

        // likes from other users
        public HashSet<Guid> LikedBy { get; private set; } = new HashSet<Guid>();

        public void Confirm()
        {
            IsConfirmed = true;
        }

        public void EditRating(UserAccount user, int newStars, string? newComment)
        {
            if (user != User)
            {
                throw new UnauthorizedAccessException("You can only edit your own ratings.");
            }

            if (newStars < 1 || newStars > 5)
            {
                throw new ArgumentOutOfRangeException("Stars´must be between 1 and 5.");
            }

            StarValue = newStars;
            Comment = newComment;
            RatingTimestamp = DateTime.Now;
            IsConfirmed = false;
        }

        public void DeleteRating()
        {
            Comment = null;
            StarValue = 0;
            IsConfirmed = false;
        }

        public void AddLike(Guid userId)
        {
            LikedBy.Add(userId);
        }

        public void RemoveLike(Guid userId)
        {
            LikedBy.Remove(userId); 
        }
    }
}
