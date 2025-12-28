using Media_Ratings_Platform.DTOs;
using MediaRatings.Domain;
using MediaRatings.Domain.interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Infrastructure.repositories
{
    public class MediaRepository : IMediaRepository
    {
        private readonly string _connectionString;

        public MediaRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int CreateMediaEntry(IMediaEntry media)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var checkCmd = new NpgsqlCommand("SELECT COUNT(*) from media WHERE title = @title AND media_type = @type", connection);
            checkCmd.Parameters.AddWithValue("title", media.Title);
            checkCmd.Parameters.AddWithValue("type", media.MediaType.ToString());

            var count = (long)checkCmd.ExecuteScalar();
            if (count > 0)
            {
                throw new InvalidOperationException($"Entry with the title '{media.Title}' with type '{media.MediaType}' already exists.");
            }

            var sql = @"
                INSERT INTO media (title, description, media_type, release_year, age_restriction, creator_user_id)
                VALUES (@title, @desc, @type, @year, @age, @creator)
                RETURNING id;";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("title", media.Title);
            cmd.Parameters.AddWithValue("desc", media.Description ?? "");
            cmd.Parameters.AddWithValue("type", media.MediaType.ToString());
            cmd.Parameters.AddWithValue("year", media.ReleaseYear);
            cmd.Parameters.AddWithValue("age", media.AgeRestriction);
            cmd.Parameters.AddWithValue("creator", media.CreatedBy);

            return (int)cmd.ExecuteScalar();
        }

        public IMediaEntry? GetMediaById(int mediaId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var sql = "SELECT * FROM media WHERE id = @id";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("id", mediaId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var mediaType = reader.GetString(reader.GetOrdinal("media_type")).ToLower();
                IMediaEntry entry = mediaType switch
                {
                    "movie" => new Movie(
                        reader.GetInt32(reader.GetOrdinal("creator_user_id")),
                        reader.GetString(reader.GetOrdinal("title")),
                        reader.GetString(reader.GetOrdinal("description")),
                        reader.GetInt32(reader.GetOrdinal("release_year")),
                        new List<Genres>(),
                        reader.GetInt32(reader.GetOrdinal("age_restriction")),
                        reader.GetInt32(reader.GetOrdinal("id")) // MediaId setzen
                    ),
                    "series" => new Series(
                        reader.GetInt32(reader.GetOrdinal("creator_user_id")),
                        reader.GetString(reader.GetOrdinal("title")),
                        reader.GetString(reader.GetOrdinal("description")),
                        reader.GetInt32(reader.GetOrdinal("release_year")),
                        new List<Genres>(),
                        reader.GetInt32(reader.GetOrdinal("age_restriction")),
                        reader.GetInt32(reader.GetOrdinal("id"))
                    ),
                    "game" => new Game(
                        reader.GetInt32(reader.GetOrdinal("creator_user_id")),
                        reader.GetString(reader.GetOrdinal("title")),
                        reader.GetString(reader.GetOrdinal("description")),
                        reader.GetInt32(reader.GetOrdinal("release_year")),
                        new List<Genres>(),
                        reader.GetInt32(reader.GetOrdinal("age_restriction")),
                        reader.GetInt32(reader.GetOrdinal("id"))
                    ),
                    _ => throw new InvalidOperationException("Unknown media type")
                };

                return entry;
            }

            return null;
        }


        public IReadOnlyCollection<IMediaEntry> GetAllMedia()
        {
            var list = new List<IMediaEntry>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var sql = @"
                SELECT id, title, description, media_type, release_year, age_restriction, creator_user_id
                FROM media";

            using var cmd = new NpgsqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var mediaType = reader.GetString(reader.GetOrdinal("media_type")).ToLower();

                IMediaEntry entry = mediaType switch
                {
                    "movie" => new Movie(
                        reader.GetInt32(reader.GetOrdinal("creator_user_id")),
                        reader.GetString(reader.GetOrdinal("title")),
                        reader.GetString(reader.GetOrdinal("description")),
                        reader.GetInt32(reader.GetOrdinal("release_year")),
                        new List<Genres>(), // Genres ggf. noch aus DB holen
                        reader.GetInt32(reader.GetOrdinal("age_restriction")),
                        reader.GetInt32(reader.GetOrdinal("id"))
                    ),
                    "series" => new Series(
                        reader.GetInt32(reader.GetOrdinal("creator_user_id")),
                        reader.GetString(reader.GetOrdinal("title")),
                        reader.GetString(reader.GetOrdinal("description")),
                        reader.GetInt32(reader.GetOrdinal("release_year")),
                        new List<Genres>(),
                        reader.GetInt32(reader.GetOrdinal("age_restriction")),
                        reader.GetInt32(reader.GetOrdinal("id"))
                    ),
                    "game" => new Game(
                        reader.GetInt32(reader.GetOrdinal("creator_user_id")),
                        reader.GetString(reader.GetOrdinal("title")),
                        reader.GetString(reader.GetOrdinal("description")),
                        reader.GetInt32(reader.GetOrdinal("release_year")),
                        new List<Genres>(),
                        reader.GetInt32(reader.GetOrdinal("age_restriction")),
                        reader.GetInt32(reader.GetOrdinal("id"))
                    ),
                    _ => throw new InvalidOperationException("Unknown media type")
                };

                list.Add(entry);
            }

            return list.AsReadOnly();
        }


        public bool UpdateMediaEntry(IMediaEntry media)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var sql = @"
                UPDATE media
                SET title = @title,
                    description = @desc,
                    media_type = @type,
                    release_year = @year,
                    age_restriction = @age
                WHERE id = @id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("title", media.Title);
            cmd.Parameters.AddWithValue("desc", media.Description ?? "");
            cmd.Parameters.AddWithValue("type", media.MediaType.ToString());
            cmd.Parameters.AddWithValue("year", media.ReleaseYear);
            cmd.Parameters.AddWithValue("age", media.AgeRestriction);
            cmd.Parameters.AddWithValue("id", media.MediaId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteMediaEntry(int mediaId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var cmd = new NpgsqlCommand(
                "DELETE FROM media WHERE id = @id", connection);

            cmd.Parameters.AddWithValue("id", mediaId);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
