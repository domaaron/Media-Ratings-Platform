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

            var mediaId = (int)cmd.ExecuteScalar();

            // save genres
            foreach (var genre in media.Genres)
            {
                using var genreCmd = new NpgsqlCommand("INSERT INTO media_genres (media_id, genre) VALUES (@mediaId, @genre)", connection);
                genreCmd.Parameters.AddWithValue("mediaId", mediaId);
                genreCmd.Parameters.AddWithValue("genre", genre.ToString().ToLower());
                genreCmd.ExecuteNonQuery();
            }

            return mediaId;
        }

        public IMediaEntry? GetMediaById(int mediaId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var sql = "SELECT * FROM media WHERE id = @id";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("id", mediaId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            var creatorId = reader.GetInt32(reader.GetOrdinal("creator_user_id"));
            var title = reader.GetString(reader.GetOrdinal("title"));
            var description = reader.GetString(reader.GetOrdinal("description"));
            var releaseYear = reader.GetInt32(reader.GetOrdinal("release_year"));
            var ageRestriction = reader.GetInt32(reader.GetOrdinal("age_restriction"));
            var id = reader.GetInt32(reader.GetOrdinal("id"));
            var mediaType = reader.GetString(reader.GetOrdinal("media_type")).ToLower();

            // keep connection for genres open, so close reader first
            reader.Close();

            // load genres
            var genres = new List<Genres>();
            using (var genreCmd = new NpgsqlCommand("SELECT genre FROM media_genres WHERE media_id = @id", connection))
            {
                genreCmd.Parameters.AddWithValue("id", mediaId);
                using var genreReader = genreCmd.ExecuteReader();
                while (genreReader.Read())
                {
                    var genreStr = genreReader.GetString(0).ToLower();
                    genres.Add(genreStr switch
                    {
                        "action" => Genres.Action,
                        "adventure" => Genres.Adventure,
                        "animation" => Genres.Animation,
                        "comedy" => Genres.Comedy,
                        "drama" => Genres.Drama,
                        "horror" => Genres.Horror,
                        "sci-fi" or "scifi" => Genres.SciFi,
                        "fantasy" => Genres.Fantasy,
                        "thriller" => Genres.Thriller,
                        "documentary" => Genres.Documentary,
                        "romance" => Genres.Romance,
                        _ => Genres.Unknown
                    });
                }
            }

            return mediaType switch
            {
                "movie" => new Movie(
                    creatorId,
                    title,
                    description,
                    releaseYear,
                    genres,
                    ageRestriction,
                    id
                ),
                "series" => new Series(
                    creatorId,
                    title,
                    description,
                    releaseYear,
                    genres,
                    ageRestriction,
                    id
                ),
                "game" => new Game(
                    creatorId,
                    title,
                    description,
                    releaseYear,
                    genres,
                    ageRestriction,
                    id
                ),
                _ => throw new InvalidOperationException("Unknown media type")
            };
        }

        public IReadOnlyCollection<IMediaEntry> GetAllMedia()
        {
            var mediaDictionary = new Dictionary<int, (int creatorId, string title, string description, string mediaType, int releaseYear, int ageRestriction, List<Genres> genres)>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var sql = @"
                SELECT m.id, m.title, m.description, m.media_type, m.release_year, m.age_restriction, m.creator_user_id,
                       mg.genre
                FROM media m
                LEFT JOIN media_genres mg ON m.id = mg.media_id
                ORDER BY m.id";

            using var cmd = new NpgsqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int mediaId = reader.GetInt32(reader.GetOrdinal("id"));
                int creatorId = reader.GetInt32(reader.GetOrdinal("creator_user_id"));
                string title = reader.GetString(reader.GetOrdinal("title"));
                string description = reader.GetString(reader.GetOrdinal("description"));
                string mediaType = reader.GetString(reader.GetOrdinal("media_type")).ToLower();
                int releaseYear = reader.GetInt32(reader.GetOrdinal("release_year"));
                int ageRestriction = reader.GetInt32(reader.GetOrdinal("age_restriction"));
                string? genreStr = reader.IsDBNull(reader.GetOrdinal("genre")) ? null : reader.GetString(reader.GetOrdinal("genre"))?.ToLower();

                if (!mediaDictionary.ContainsKey(mediaId))
                {
                    mediaDictionary[mediaId] = (creatorId, title, description, mediaType, releaseYear, ageRestriction, new List<Genres>());
                }

                if (!string.IsNullOrEmpty(genreStr))
                {
                    mediaDictionary[mediaId].genres.Add(genreStr switch
                    {
                        "action" => Genres.Action,
                        "adventure" => Genres.Adventure,
                        "animation" => Genres.Animation,
                        "comedy" => Genres.Comedy,
                        "drama" => Genres.Drama,
                        "horror" => Genres.Horror,
                        "sci-fi" or "scifi" => Genres.SciFi,
                        "fantasy" => Genres.Fantasy,
                        "thriller" => Genres.Thriller,
                        "documentary" => Genres.Documentary,
                        "romance" => Genres.Romance,
                        _ => Genres.Unknown
                    });
                }
            }

            var list = new List<IMediaEntry>();
            foreach (var kvp in mediaDictionary)
            {
                var id = kvp.Key;
                var data = kvp.Value;

                IMediaEntry entry = data.mediaType switch
                {
                    "movie" => new Movie(data.creatorId, data.title, data.description, data.releaseYear, data.genres, data.ageRestriction, id),
                    "series" => new Series(data.creatorId, data.title, data.description, data.releaseYear, data.genres, data.ageRestriction, id),
                    "game" => new Game(data.creatorId, data.title, data.description, data.releaseYear, data.genres, data.ageRestriction, id),
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

            // synchronize genres
            using var delCmd = new NpgsqlCommand("DELETE FROM media_genres WHERE media_id = @id", connection);
            delCmd.Parameters.AddWithValue("id", media.MediaId);
            delCmd.ExecuteNonQuery();

            foreach (var genre in media.Genres)
            {
                using var genreCmd = new NpgsqlCommand("INSERT INTO media_genres (media_id, genre) VALUES (@id, @genre)", connection);
                genreCmd.Parameters.AddWithValue("id", media.MediaId);
                genreCmd.Parameters.AddWithValue("genre", genre.ToString().ToLower());
                genreCmd.ExecuteNonQuery();
            }


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
