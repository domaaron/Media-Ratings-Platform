using Media_Ratings_Platform.DTOs;
using MediaRatings.Domain;
using MediaRatings.Domain.interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Media_Ratings_Platform.Test.fakes
{
    public class InMemoryMediaManager : IMediaManager
    {
        private readonly List<IMediaEntry> _mediaEntries = new();

        public void AddMediaEntry(IMediaEntry mediaEntry)
        {
            _mediaEntries.Add(mediaEntry);
        }

        public bool RemoveMediaEntry(int mediaEntryId)
        {
            var mediaEntry = _mediaEntries.FirstOrDefault(m => m.MediaId == mediaEntryId);
            if (mediaEntry == null)
                return false;

            _mediaEntries.Remove(mediaEntry);
            return true;
        }

        public bool UpdateMediaEntry(int oldMediaEntryId, UpdateMediaDto mediaDto)
        {
            var mediaEntry = _mediaEntries.FirstOrDefault(m => m.MediaId == oldMediaEntryId);
            if (mediaEntry == null)
                return false;

            mediaEntry.Title = mediaDto.Title;
            mediaEntry.Description = mediaDto.Description;
            mediaEntry.ReleaseYear = mediaDto.ReleaseYear;
            mediaEntry.Genres = mediaDto.Genres
                .Select(g => Enum.Parse<Genres>(g))
                .ToList();

            return true;
        }

        public IMediaEntry? GetMediaById(int mediaEntryId)
        {
            return _mediaEntries.FirstOrDefault(m => m.MediaId == mediaEntryId);
        }

        public IReadOnlyCollection<IMediaEntry> GetAllMediaEntries()
        {
            return _mediaEntries.AsReadOnly();
        }

        public int CountMediaEntries()
        {
            return _mediaEntries.Count;
        }
    }
}
