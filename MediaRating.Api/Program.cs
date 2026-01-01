using Media_Ratings_Platform.DTOs;
using Media_Ratings_Platform.services;
using MediaRatings.Api;
using MediaRatings.Api.controllers;
using MediaRatings.Domain;
using MediaRatings.Domain.repositories;
using MediaRatings.Domain.services;
using MediaRatings.Infrastructure;
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

// DB init
var dbInitializer = new DatabaseInitializer(connectionString);
dbInitializer.Initialize();

// generate a secure random JWT secret (32 bytes = 256 bits)
var jwtSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

// initialize dependencies
var userRepository = new UserRepository(connectionString);
var authService = new AuthService(userRepository, jwtSecret);
var jwtService = new JwtService(jwtSecret);
var mediaRepository = new MediaRepository(connectionString);
var ratingRepository = new RatingRepository(connectionString, mediaRepository, userRepository);
var favoritesRepository = new FavoritesRepository(connectionString, mediaRepository);
var mediaManager = new MediaManager(mediaRepository);
var favoritesManager = new FavoritesManager(favoritesRepository);
var ratingManager = new RatingManager(ratingRepository);

// controllers
var authController = new AuthController(authService);
var userController = new UserController(jwtService, userRepository, ratingManager, favoritesManager, mediaManager);
var mediaController = new MediaController(mediaManager, jwtService, userRepository, ratingManager);
var ratingController = new RatingController(ratingManager, mediaManager, userRepository, jwtService);
var favoritesController = new FavoritesController(favoritesManager, jwtService);

// router
var router = new Router();

// authentication
router.Register("POST", "/api/users/register", authController.RegisterAsync);
router.Register("POST", "/api/users/login", authController.LoginAsync);

// profile
router.Register("GET", "/api/users/{id}/profile", userController.GetProfileAsync);
router.Register("GET", "/api/users/{id}/ratings", userController.GetRatingHistoryAsync);
router.Register("GET", "/api/users/{id}/favorites", userController.GetFavoritesAsync);
router.Register("PUT", "/api/users/{id}/profile", userController.UpdateProfileAsync);

// recommendations
router.Register("GET", "/api/users/{id}/recommendations", userController.GetRecommendationsAsync);

// media management
router.Register("POST", "/api/media", mediaController.CreateMediaAsync);
router.Register("GET", "/api/media", mediaController.GetAllMediaAsync);
router.Register("GET", "/api/media/{id}", mediaController.GetMediaByIdAsync);
router.Register("PUT", "/api/media/{id}", mediaController.UpdateMediaAsync);
router.Register("DELETE", "/api/media/{id}", mediaController.DeleteMediaAsync);

// rating management
router.Register("POST", "/api/media/{id}/rate", ratingController.RateMediaAsync);
router.Register("PUT", "/api/ratings/{ratingId}", ratingController.EditRatingAsync);
router.Register("POST", "/api/ratings/{ratingId}/like", ratingController.LikeRatingAsync);
router.Register("POST", "/api/ratings/{ratingId}/confirm", ratingController.ConfirmRatingAsync);

// favorites management
router.Register("POST", "/api/media/{id}/favorite", favoritesController.AddFavoriteAsync);
router.Register("DELETE", "/api/media/{id}/favorite", favoritesController.RemoveFavoriteAsync);

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
