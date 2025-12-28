using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.interfaces
{
    public interface IMediaRepository
    {
        int CreateMediaEntry(IMediaEntry media);
        IMediaEntry? GetMediaById(int mediaId);
        IReadOnlyCollection<IMediaEntry> GetAllMedia();
        bool UpdateMediaEntry(IMediaEntry media);
        bool DeleteMediaEntry(int mediaId);
    }
}
