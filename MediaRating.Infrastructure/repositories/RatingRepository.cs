using MediaRatings.Domain;
using MediaRatings.Domain.interfaces;
using MediaRatings.Domain.repositories;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Infrastructure.repositories
{
    /*
    Repository for managing user ratings in the database:
        - Provides CRUD operations for UserRating objects
        - Methods:
            • AddRatingAsync(rating) → inserts a new rating into the database, returns the new rating ID
            • UpdateRatingAsync(rating) → updates an existing rating, including stars, comment, and confirmation status
            • DeleteRatingAsync(ratingId) → deletes a rating by ID
            • GetRatingByIdAsync(ratingId) → retrieves a single rating by ID
            • GetRatingByUserAsync(userId) → retrieves all ratings by a specific user
            • GetRatingByMediaAsync(mediaId) → retrieves all ratings for a specific media entry
        - Relies on IMediaRepository and IUserRepository to fetch associated media and user objects
    */
    public class RatingRepository : IRatingRepository
    {
        private readonly string _connectionString;
        private readonly IMediaRepository _mediaRepository;
        private readonly IUserRepository _userRepository;

        public RatingRepository(string connectionString, IMediaRepository mediaRepository, IUserRepository userRepository)
        {
            _connectionString = connectionString;
            _mediaRepository = mediaRepository;
            _userRepository = userRepository;
        }

        public async Task<int> AddRatingAsync(UserRating rating)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO ratings (media_id, user_id, stars, comment)
                VALUES (@media, @user, @stars, @comment)
                RETURNING id;";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("media", rating.MediaEntry.MediaId);
            cmd.Parameters.AddWithValue("user", rating.User.UserId);
            cmd.Parameters.AddWithValue("stars", rating.StarValue);
            cmd.Parameters.AddWithValue("comment", rating.Comment ?? "");

            return (int)await cmd.ExecuteScalarAsync();
        }

        public async Task UpdateRatingAsync(UserRating rating)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                UPDATE ratings
                SET stars = @stars,
                    comment = @comment,
                    is_confirmed = @isConfirmed
                WHERE id = @id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("stars", rating.StarValue);
            cmd.Parameters.AddWithValue("comment", rating.Comment ?? "");
            cmd.Parameters.AddWithValue("isConfirmed", rating.IsConfirmed);
            cmd.Parameters.AddWithValue("id", rating.RatingId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteRatingAsync(int ratingId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "DELETE FROM ratings WHERE id = @id";
            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("id", ratingId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<UserRating?> GetRatingByIdAsync(int ratingId)
        {
            var ratings = await GetRatingsAsync("id", ratingId);
            return ratings.FirstOrDefault();
        }

        public async Task<IReadOnlyCollection<UserRating>> GetRatingByUserAsync(int userId)
        {
            return await GetRatingsAsync("user_id", userId);
        }

        public async Task<IReadOnlyCollection<UserRating>> GetRatingByMediaAsync(int mediaId)
        {
            return await GetRatingsAsync("media_id", mediaId);
        }

        private async Task<IReadOnlyCollection<UserRating>> GetRatingsAsync(string columnnName, int id)
        {
            var list = new List<UserRating>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = $"SELECT * FROM ratings WHERE {columnnName} = @id";
            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var media = _mediaRepository.GetMediaById(
                    reader.GetInt32(reader.GetOrdinal("media_id"))
                );

                var user = await _userRepository.GetByIdAsync(
                    reader.GetInt32(reader.GetOrdinal("user_id"))
                );

                list.Add(new UserRating(
                    reader.GetInt32(reader.GetOrdinal("id")),
                    media!,
                    user!,
                    reader.GetInt32(reader.GetOrdinal("stars")),
                    reader.IsDBNull("comment") ? null : reader.GetString("comment"),
                    reader.GetDateTime(reader.GetOrdinal("created_at")),
                    reader.GetBoolean(reader.GetOrdinal("is_confirmed"))
                ));
            }

            return list.AsReadOnly();
        }
    }
}
