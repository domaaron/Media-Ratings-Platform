using MediaRatings.Infrastructure.security;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Infrastructure
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Initialize()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // drop & create tables
            using var cmd = new NpgsqlCommand(@"
            DROP TABLE IF EXISTS media_genres CASCADE;
            DROP TABLE IF EXISTS favorites CASCADE;
            DROP TABLE IF EXISTS ratings CASCADE;
            DROP TABLE IF EXISTS media CASCADE;
            DROP TABLE IF EXISTS users CASCADE;

            CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                username TEXT UNIQUE NOT NULL,
                password_hash TEXT NOT NULL,
                email TEXT,
                favorite_genre TEXT
            );

            CREATE TABLE media (
                id SERIAL PRIMARY KEY,
                title TEXT NOT NULL,
                description TEXT,
                media_type TEXT NOT NULL,
                release_year INT,
                age_restriction INT,
                creator_user_id INT REFERENCES users(id) ON DELETE CASCADE
            );

            CREATE TABLE ratings (
                id SERIAL PRIMARY KEY,
                stars INT CHECK (stars BETWEEN 1 AND 5),
                comment TEXT,
                is_confirmed BOOLEAN DEFAULT FALSE,
                created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                user_id INT REFERENCES users(id) ON DELETE CASCADE,
                media_id INT REFERENCES media(id) ON DELETE CASCADE
            );

            CREATE TABLE favorites (
                user_id INT REFERENCES users(id) ON DELETE CASCADE,
                media_id INT REFERENCES media(id) ON DELETE CASCADE,
                PRIMARY KEY (user_id, media_id)
            );

            CREATE TABLE media_genres (
                media_id INT REFERENCES media(id) ON DELETE CASCADE,
                genre TEXT NOT NULL,
                PRIMARY KEY (media_id, genre)
            );
            ", connection);
            cmd.ExecuteNonQuery();

            // seed Users
            using var insertUserCmd = new NpgsqlCommand(@"
            INSERT INTO users (username, password_hash, email, favorite_genre)
            VALUES (@username, @password, @email, @genre)
            RETURNING id;
            ", connection);

            insertUserCmd.Parameters.AddWithValue("username", "admin");
            insertUserCmd.Parameters.AddWithValue("password", PasswordHasher.HashPassword("admin123"));
            insertUserCmd.Parameters.AddWithValue("email", DBNull.Value);
            insertUserCmd.Parameters.AddWithValue("genre", DBNull.Value);
            var userId = (int)insertUserCmd.ExecuteScalar();

            // seed media entries + genres
            var mediaEntries = new List<(string Title, string Description, string Type, int Year, int Age, string[] Genres)>
            {
                ("Avatar: Fire and Ash", "blue people", "Movie", 2025, 12, new [] { "animation", "adventure" }),
                ("Fínal Fantasy VII", "Fantasy RPG", "Game", 1997, 12, new [] { "fantasy" }),
                ("Inception", "Sci-fi thriller", "Movie", 2010, 12, new [] { "sci-fi" })
            };

            foreach (var media in mediaEntries)
            {
                var mediaId = (int)new NpgsqlCommand(@"
                INSERT INTO media (title, description, media_type, release_year, age_restriction, creator_user_id)
                VALUES (@title, @desc, @type, @year, @age, @creator)
                RETURNING id;
                ", connection)
                {
                    Parameters =
                {
                    new NpgsqlParameter("title", media.Title),
                    new NpgsqlParameter("desc", media.Description),
                    new NpgsqlParameter("type", media.Type),
                    new NpgsqlParameter("year", media.Year),
                    new NpgsqlParameter("age", media.Age),
                    new NpgsqlParameter("creator", userId)
                }}
                .ExecuteScalar();

                foreach (var genre in media.Genres)
                {
                    using var genreCmd = new NpgsqlCommand(@"
                    INSERT INTO media_genres (media_id, genre)
                    VALUES (@id, @genre);
                    ", connection);
                    genreCmd.Parameters.AddWithValue("id", mediaId);
                    genreCmd.Parameters.AddWithValue("genre", genre);
                    genreCmd.ExecuteNonQuery();
                }
            }
        }
    }

}
