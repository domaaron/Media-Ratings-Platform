using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MediaRatings.Api.Utils
{
    public class JsonHelper
    {
        public static readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public static async Task<T?> ReadBodyAsync<T>(HttpListenerRequest request)
        {
            using var reader = new StreamReader(request.InputStream);
            var body = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<T>(body, options);
        }
    }
}
