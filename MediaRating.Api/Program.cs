using Media_Ratings_Platform.DTOs;
using Media_Ratings_Platform.services;
using MediaRatings.Api;
using MediaRatings.Api.controllers;
using MediaRatings.Domain;
using MediaRatings.Domain.services;
using MediaRatings.Infrastructure.repositories;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// ------------------------------------------------
// setup
// ------------------------------------------------

// connection string for PostgreSQL database
var connectionString = "Host=localhost;Database=mrp;Username=postgres;Password=1234";

// generate a secure random JWT secret (32 bytes = 256 bits)
var jwtSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

// initialize dependencies
var userRepository = new UserRepository(connectionString);
var authService = new AuthService(userRepository, jwtSecret);
var jwtService = new JwtService(jwtSecret);
var mediaManager = new MediaManager();
var favoritesManager = new FavoritesManager();
var ratingManager = new RatingManager();

// controllers
var authController = new AuthController(authService);
var userController = new UserController(jwtService, userRepository, ratingManager, favoritesManager);
var mediaController = new MediaController(mediaManager, jwtService);

// router
var router = new Router();
router.Register("POST", "/api/users/register", authController.RegisterAsync);
router.Register("POST", "/api/users/login", authController.LoginAsync);
router.Register("GET", "/api/users/{id}/profile", userController.GetProfileAsync);
router.Register("POST", "/api/media", mediaController.CreateMediaAsync);
router.Register("GET", "/api/media", mediaController.GetAllMediaAsync);
router.Register("GET", "/api/media/{id}", mediaController.GetMediaByIdAsync);
router.Register("PUT", "/api/media/{id}", mediaController.UpdateMediaAsync);
router.Register("DELETE", "/api/media/{id}", mediaController.DeleteMediaAsync);

// start http server
var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:8080/");
listener.Start();
Console.WriteLine("Server läuft auf http://localhost:8080/");


// ------------------ Main loop ------------------
while (true)
{
    var context = await listener.GetContextAsync();
    await router.HandleRequestAsync(context);
}
