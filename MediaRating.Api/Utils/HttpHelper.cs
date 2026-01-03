using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MediaRatings.Api.Utils
{
    /*
    Helper class for writing HTTP responses:
        - Provides methods to send JSON or plain text responses
        - Automatically sets HTTP status code and Content-Type header
    */
    public class HttpHelper
    {
        public static async Task WriteJsonAsync(HttpListenerResponse response, int statusCode, object data)
        {
            response.StatusCode = statusCode;
            response.ContentType = "application/json";
            var json = JsonSerializer.Serialize(data);
            await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(json));
            response.Close();
        }

        public static async Task WriteTextAsync(HttpListenerResponse response, int statusCode, string message)
        {
            response.StatusCode = statusCode;
            await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(message));
            response.Close();
        }
    }
}
