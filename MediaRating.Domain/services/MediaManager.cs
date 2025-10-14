using Media_Ratings_Platform.DTOs;
using MediaRatings.Domain.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.services
{
    public class MediaManager : IMediaManager
    {
        // media created by own user
        private readonly List<IMediaEntry> _mediaEntries = new();

        public IMediaEntry? GetMediaById(int mediaEntryId)
        {
            return _mediaEntries.OfType<MediaEntry>().FirstOrDefault(m => m.MediaId == mediaEntryId);
        }

        public void AddMediaEntry(IMediaEntry mediaEntry)
        {
            if (!_mediaEntries.Contains(mediaEntry))
            {
                _mediaEntries.Add(mediaEntry);
            }
        }

        public bool RemoveMediaEntry(int mediaEntryId)
        {
            var entry = GetMediaById(mediaEntryId);
            if (entry == null) return false;

            _mediaEntries.Remove(entry);
            return true;
        }

        public bool UpdateMediaEntry(int oldMediaEntry, UpdateMediaDto mediaDto)
        {
            var entry = _mediaEntries.OfType<MediaEntry>().FirstOrDefault(m => m.MediaId == oldMediaEntry);
            if (entry == null)
            {
                return false;
            }

            Genres ParseGenre(string g) => g.ToLower() switch
            {
                "action" => Genres.Action,
                "thriller" => Genres.Thriller,
                "sci-fi" or "scifi" => Genres.SciFi,
                "animation" => Genres.Animation,
                "comedy" => Genres.Comedy,
                "drama" => Genres.Drama,
                "fantasy" => Genres.Fantasy,
                "adventure" => Genres.Adventure,
                _ => Genres.Unknown
            };

            entry.Title = mediaDto.Title;
            entry.Description = mediaDto.Description;
            entry.ReleaseYear = mediaDto.ReleaseYear;
            entry.AgeRestriction = mediaDto.AgeRestriction;
            entry.Genres = mediaDto.Genres.Select(ParseGenre).ToList();

            return true;
        }

        public int CountMediaEntries()
        {
            return _mediaEntries.Count;
        }

        public IReadOnlyCollection<IMediaEntry> GetAllMediaEntries()
        {
            return _mediaEntries.AsReadOnly();
        }
    }
}
