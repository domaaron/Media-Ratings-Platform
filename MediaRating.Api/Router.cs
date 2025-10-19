using MediaRatings.Api.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MediaRatings.Api
{
    public class Router
    {
        private readonly List<Route> _routes = new();
        public void Register(string method, string path, Func<HttpListenerContext, Task> handler)
        {
            _routes.Add(new Route(method, path, handler));
        }

        public async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var route = _routes.FirstOrDefault(r => r.Method == request.HttpMethod && r.IsMatch(request.Url.AbsolutePath));
                
                if (route == null)
                {
                    await HttpHelper.WriteTextAsync(context.Response, 404, "Not found");
                    return;
                }

                await route.Handler(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await HttpHelper.WriteTextAsync(context.Response, 500, "Server error: " + ex.Message);
            }
        }

        public record Route(string Method, string Path, Func<HttpListenerContext, Task> Handler)
        {
            public bool IsMatch(string urlPath)
            {
                var routeSegments = Path.Trim('/').Split('/');
                var urlSegments = urlPath.Trim('/').Split('/');

                if (routeSegments.Length != urlSegments.Length)
                {
                    return false;
                }

                for (int i = 0; i < routeSegments.Length; i++)
                {
                    var routeSegment = routeSegments[i];
                    var urlSegment = urlSegments[i];

                    if (routeSegment.StartsWith("{") && routeSegment.EndsWith("}"))
                    {
                        continue;
                    }

                    if (!routeSegment.Equals(urlSegment, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
