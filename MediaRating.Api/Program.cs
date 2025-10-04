using Media_Ratings_Platform.DTOs;
using MediaRatings.Domain.services;
using MediaRatings.Infrastructure.repositories;
using System.Net;
using System.Security.Cryptography;
using System.Text;

// connection string for PostgreSQL database
var connectionString = "Host=localhost;Database=mrp;Username=postgres;Password=1234";

// generate a secure random JWT secret (32 bytes = 256 bits)
var jwtSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

// initialize repository and authentication service
var userRepository = new UserRepository(connectionString);
var authService = new AuthService(userRepository, jwtSecret);

// create an HTTP listener on localhost
var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:8080/");
listener.Start();

Console.WriteLine("Server läuft auf http://localhost:8080/");

// infinite loop to handle incoming requests
while (true)
{
    var context = await listener.GetContextAsync();
    var request = context.Request;
    var response = context.Response;

    // handle user registration
    if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/api/users/register")
    {
        // read request body
        using var reader = new StreamReader(request.InputStream);
        var body = await reader.ReadToEndAsync();

        // JSON deserialization options (case-insensitive property matching)
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        // deserialize the request body into RegisterDto
        var data = System.Text.Json.JsonSerializer.Deserialize<RegisterDto>(body, options);

        // debugging
        Console.WriteLine("Body: " + body);
        Console.WriteLine("Deserialized: " + data?.Username + " / " + data?.Password);

        try
        {
            // attempt registration
            var user = await authService.RegisterUserAsync(data.Username, data.Password);

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
    }
    // handle user login
    else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/api/users/login")
    {
        using var reader = new StreamReader(request.InputStream);
        var body = await reader.ReadToEndAsync();
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var data = System.Text.Json.JsonSerializer.Deserialize<LoginDto>(body, options);


        // attempt to log in the user and generate JWT token
        var token = await authService.LoginAsync(data.Username, data.Password);
        if (token == null)
        {
            // invalid credentials, return HTTP 401 Unauthorized
            response.StatusCode = 401;
        }
        else
        {
            // login successful, return HTTP 200 OK with JWT token
            response.StatusCode = 200;
            var json = System.Text.Json.JsonSerializer.Serialize(new { token });
            await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(json));
        }
        response.Close();
    }
    // handle unknown endpoints
    else
    {
        response.StatusCode = 404;
        response.Close();
    }
}
