using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Ratings_Platform.DTOs
{
    public class UpdateProfileDto
    {
        public string? Email { get; set; }
        public string? FavoriteGenre { get; set; }
    }
}
