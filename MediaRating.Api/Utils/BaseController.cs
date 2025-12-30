using Media_Ratings_Platform.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Api.Utils
{
    public abstract class BaseController
    {
        protected readonly JwtService _jwtService;
        protected BaseController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        protected async Task<int?> AuthenticateAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
            var authHeader = request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                await HttpHelper.WriteTextAsync(response, 401, "Unauthorized: missing token");
                return null;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var userData = _jwtService.ValidateToken(token);
            if (userData == null)
            {
                await HttpHelper.WriteTextAsync(response, 401, "Unauthorized: Invalid token");
                return null;
            }

            return userData.Value.UserId;
        }

        protected static int? ExtractId(string path)
        {
            var parts = path.Trim('/').Split('/');

            if (parts.Length < 3)
            {
                return null;
            }

            return int.TryParse(parts[2], out var id) ? id : null;
        }
    }
}
