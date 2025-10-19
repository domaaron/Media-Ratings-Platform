using Media_Ratings_Platform.DTOs;
using Media_Ratings_Platform.services;
using MediaRatings.Domain;
using MediaRatings.Domain.services;
using MediaRatings.Infrastructure.repositories;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// connection string for PostgreSQL database
var connectionString = "Host=localhost;Database=mrp;Username=postgres;Password=1234";

// generate a secure random JWT secret (32 bytes = 256 bits)
var jwtSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

// initialize repository and authentication service
var userRepository = new UserRepository(connectionString);
var authService = new AuthService(userRepository, jwtSecret);
var jwtService = new JwtService(jwtSecret);

// initialize managers (CRUD, Ratings, Favorites)
var mediaManager = new MediaManager();
var favoritesManager = new FavoritesManager();
var ratingManager = new RatingManager();

// create an HTTP listener on localhost
var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:8080/");
listener.Start();
Console.WriteLine("Server läuft auf http://localhost:8080/");

// ------------------ JWT Auth Helper ------------------
UserAccount? Authenticate(HttpListenerRequest request)
{
    var authHeader = request.Headers["Authorization"] ?? request.Headers["authorization"];
    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
    {
        Console.WriteLine("No Authorization header or not Bearer");
        return null;
    }

    var token = authHeader.Substring("Bearer ".Length).Trim();
    var username = jwtService.ValidateToken(token);
    if (username == null)
    {
        Console.WriteLine("Token invalid");
        return null;
    }

    // debugging
    Console.WriteLine("Auth Header: " + authHeader);
    Console.WriteLine("Token: " + token);
    Console.WriteLine("Username: " + username);

    return userRepository.FindByUsernameAsync(username).Result;
}


// ------------------ Main loop ------------------
while (true)
{
    var context = await listener.GetContextAsync();
    var request = context.Request;
    var response = context.Response;

    try
    {
        string path = request.Url.AbsolutePath;

        // ------------------ Auth ------------------
        if (request.HttpMethod == "POST" && path == "/api/users/register")
        {
            // read request body
            using var reader = new StreamReader(request.InputStream);
            var body = await reader.ReadToEndAsync();

            // JSON deserialization options (case-insensitive property matching)
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            // deserialize the request body into RegisterDto
            var data = JsonSerializer.Deserialize<RegisterDto>(body, options);

            // debugging
            Console.WriteLine("Body: " + body);
            Console.WriteLine("Deserialized: " + data?.Username + " / " + data?.Password);

            try
            {
                // attempt registration
                var newUser = await authService.RegisterUserAsync(data.Username, data.Password);

                // respond with HTTP 201 Created
                response.StatusCode = 201;
                await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("User created"));
            }
            catch (InvalidOperationException ex)
            {
                // username already exists, return HTTP 409 Conflict
                response.StatusCode = 409;
                await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(ex.Message));
            }
            response.Close();
            continue;
        }

        if (request.HttpMethod == "POST" && path == "/api/users/login")
        {
            using var reader = new StreamReader(request.InputStream);
            var body = await reader.ReadToEndAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var data = JsonSerializer.Deserialize<LoginDto>(body, options);


            // attempt to log in the user and generate JWT token
            var token = await authService.LoginAsync(data.Username, data.Password);
            Console.WriteLine("Generated token: " + token);
            if (token == null)
            {
                // invalid credentials, return HTTP 401 Unauthorized
                response.StatusCode = 401;
            }
            else
            {
                // login successful, return HTTP 200 OK with JWT token
                response.StatusCode = 200;
                response.ContentType = "application/json";
                var json = JsonSerializer.Serialize(new { token });
                await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(json));
            }
            response.Close();
            continue;
        }

        // ------------------ JWT Auth Check ------------------
        var user = Authenticate(request);
        if (path != "/api/users/login" && path != "/api/users/register")
        {
            if (user == null)
            {
                response.StatusCode = 401;
                await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Unauthorized"));
                response.Close();
                continue;
            }
        }

        // ------------------ User profile endpoints ------------------
        if (request.HttpMethod == "GET" && path.StartsWith("/api/users/") && path.EndsWith("/profile"))
        {
            var userIdString = path.Split("/")[3];
            if (!int.TryParse(userIdString, out var userId) || userId != user.UserId)
            {
                response.StatusCode = 403;
                response.Close();
                continue;
            }

            var profileDto = new
            {
                user.UserId,
                user.Username,
                TotalRatings = ratingManager.CountRatings(),
                AverageScore = ratingManager.AverageRatingGiven(),
                Favorites = favoritesManager.CountFavorites()
            };

            response.StatusCode = 200;
            response.ContentType = "application/json";
            var json = JsonSerializer.Serialize(profileDto);
            await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(json));
            response.Close();
            continue;
        }

        // ------------------ Media endpoints ------------------

        // POST /api/media
        if (request.HttpMethod == "POST" && path == "/api/media")
        {
            using var reader = new StreamReader(request.InputStream);
            var body = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var mediaDto = JsonSerializer.Deserialize<CreateMediaDto>(body, options);
            Console.WriteLine($"MediaType raw: '{mediaDto?.MediaType}'");


            if (mediaDto == null)
            {
                response.StatusCode = 400;
                await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Invalid media data"));
                response.Close();
                continue;
            }

            Genres ParseGenre(string g) => g.ToLower() switch
            {
                "action" => Genres.Action,
                "thriller" => Genres.Thriller,
                "sci-fi" => Genres.SciFi,
                "animation" => Genres.Animation,
                "comedy" => Genres.Comedy,
                "drama" => Genres.Drama,
                "fantasy" => Genres.Fantasy,
                "adventure" => Genres.Adventure,
                _ => Genres.Unknown
            };

            var genres = mediaDto.Genres.Select(ParseGenre).ToList();

            var mediaTypeString = mediaDto.MediaType?.Trim().ToLower();
            MediaEntry entry = mediaTypeString switch
            {
                "movie" => new Movie(user.UserId, mediaDto.Title, mediaDto.Description, mediaDto.ReleaseYear, genres, mediaDto.AgeRestriction),
                "series" => new Series(user.UserId, mediaDto.Title, mediaDto.Description, mediaDto.ReleaseYear, genres, mediaDto.AgeRestriction),
                "game" => new Game(user.UserId, mediaDto.Title, mediaDto.Description, mediaDto.ReleaseYear, genres, mediaDto.AgeRestriction),
                _ => throw new InvalidOperationException($"Unknown media type '{mediaTypeString}'")
            };


            mediaManager.AddMediaEntry(entry);

            response.StatusCode = 201;
            response.ContentType = "application/json";
            var json = JsonSerializer.Serialize(new
            {
                entry.MediaId,
                entry.CreatedBy,
                entry.Title,
                entry.Description,
                MediaType = entry.MediaType.ToString().ToLower(),
                entry.ReleaseYear,
                Genres = entry.Genres.Select(g => g.ToString().ToLower()),
                entry.AgeRestriction
            });
            await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(json));
            response.Close();
            continue;
        }

        // GET /api/media/ -> all media entries
        else if (request.HttpMethod == "GET" && path == "/api/media")
        {
            //var entries = mediaManager.GetAllMediaEntries();
            var entries = mediaManager.GetAllMediaEntries()
                .OfType<MediaEntry>()   // nur MediaEntry
                .Select(e => new
                {
                    title = e.Title,
                    description = e.Description,
                    mediaType = e.MediaType.ToString().ToLower(),
                    releaseYear = e.ReleaseYear,
                    genres = e.Genres.Select(g => g.ToString().ToLower()),
                    ageRestriction = e.AgeRestriction
                });
            response.StatusCode = 200;
            response.ContentType = "application/json";
            var json = JsonSerializer.Serialize(entries);
            await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(json));
            response.Close();
            continue;
        }

        // GET /api/media/{id}, PUT / DELETE /api/media/{id}
        else if (path.StartsWith("/api/media"))
        {
            var idPart = path.Replace("/api/media/", "");
            if (!int.TryParse(idPart, out var mediaId))
            {
                response.StatusCode = 400;
                await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Invalid media ID"));
                response.Close();
                continue;
            }

            if (request.HttpMethod == "GET")
            {
                var entry = mediaManager.GetMediaById(mediaId) as MediaEntry;
                if (entry == null)
                {
                    response.StatusCode = 404;
                    await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Media not found"));
                    response.Close();
                    continue;
                }

                response.StatusCode = 200;
                response.ContentType = "application/json";
                var json = JsonSerializer.Serialize(new
                {
                    title = entry.Title,
                    description = entry.Description,
                    mediaType = entry.MediaType.ToString().ToLower(),
                    releaseYear = entry.ReleaseYear,
                    genres = entry.Genres.Select(g => g.ToString().ToLower()),
                    ageRestriction = entry.AgeRestriction
                });
                await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(json));
                response.Close();
                continue;
            }

            if (request.HttpMethod == "PUT")
            {
                using var reader = new StreamReader(request.InputStream);
                var body = await reader.ReadToEndAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var updateDto = JsonSerializer.Deserialize<UpdateMediaDto>(body, options);
                if (updateDto == null)
                {
                    response.StatusCode = 404;
                    await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Invalid media data"));
                    response.Close();
                    continue;
                }

                var updated = mediaManager.UpdateMediaEntry(mediaId, updateDto);
                if (!updated)
                {
                    response.StatusCode = 404;
                    await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Media not found"));
                    response.Close();
                    continue;
                }

                var updatedEntry = mediaManager.GetMediaById(mediaId) as MediaEntry;

                var mediaDto = new UpdateMediaDto
                {
                    MediaId = updatedEntry.MediaId,
                    CreatedBy = updatedEntry.CreatedBy,
                    Title = updatedEntry.Title,
                    Description = updatedEntry.Description,
                    MediaType = updatedEntry.MediaType.ToString().ToLower(),
                    ReleaseYear = updatedEntry.ReleaseYear,
                    Genres = updatedEntry.Genres.Select(g => g.ToString().ToLower()).ToList(),
                    AgeRestriction = updatedEntry.AgeRestriction
                };
                
                response.StatusCode = 200;
                response.ContentType = "application/json";
                var json = JsonSerializer.Serialize(mediaDto);
                await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(json));
                response.Close();
                continue;
            }

            if (request.HttpMethod == "DELETE")
            {
                var deleted = mediaManager.RemoveMediaEntry(mediaId);
                response.StatusCode = deleted ? 204 : 404;
                response.Close();
                continue;
            }
        }

        // ------------------ Default 404 ------------------
        response.StatusCode = 404;
        await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Not found"));
        response.Close();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        response.StatusCode = 500;
        await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Server error: " + ex.Message));
        response.Close();
    }
}
