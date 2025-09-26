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
        public void AddMediaEntry(IMediaEntry mediaEntry)
        {
            if (!_mediaEntries.Contains(mediaEntry))
            {
                _mediaEntries.Add(mediaEntry);
            }
        }

        public void RemoveMediaEntry(IMediaEntry mediaEntry)
        {
            if (_mediaEntries.Contains(mediaEntry))
            {
                _mediaEntries.Remove(mediaEntry);
            }
        }

        public void UpdateMediaEntry(IMediaEntry oldEntry, IMediaEntry newEntry)
        {
            int index = _mediaEntries.IndexOf(oldEntry);
            if (index >= 0)
            {
                _mediaEntries[index] = newEntry;
            }
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
