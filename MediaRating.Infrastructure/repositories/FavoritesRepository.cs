using MediaRatings.Domain;
using MediaRatings.Domain.interfaces;
using MediaRatings.Domain.repositories;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Infrastructure.repositories
{
    public class FavoritesRepository : IFavoritesRepository
    {
        private readonly string _connectionString;
        private readonly IMediaRepository _mediaRepository;

        public FavoritesRepository(string connectionString, IMediaRepository mediaRepository)
        {
            _connectionString = connectionString;
            _mediaRepository = mediaRepository;
        }

        public async Task AddFavoriteAsync(int userId, int mediaId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO favorites (user_id, media_id)
                VALUES (@user, @media);";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("user", userId);
            cmd.Parameters.AddWithValue("media", mediaId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveFavoriteAsync(int userId, int mediaId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "DELETE FROM favorites WHERE user_id = @user AND media_id = @media;";
            await using var cmd = new NpgsqlCommand( sql, connection);
            cmd.Parameters.AddWithValue("user", userId);
            cmd.Parameters.AddWithValue("media", mediaId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IReadOnlyCollection<IMediaEntry>> GetFavoritesByUserAsync(int userId)
        {
            var list = new List<IMediaEntry>();

            await using var connection = new NpgsqlConnection( _connectionString);
            await connection.OpenAsync();

            var sql = "SELECT media_id FROM favorites WHERE user_id = @user;";
            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("user", userId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int mediaId = reader.GetInt32(0);
                var media = _mediaRepository.GetMediaById(mediaId);
                if (media != null)
                {
                    list.Add(media);
                }
            }

            return list.AsReadOnly();
        }

        public async Task<bool> IsFavoritesAsync(int userId, int mediaId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "SELECT 1 FROM favorites WHERE user_id = @user AND media_id = @media;";
            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("user", userId);
            cmd.Parameters.AddWithValue("media", mediaId);

            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }
    }
}
