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

            // seed users
            using var insertUserCmd = new NpgsqlCommand(@"
                INSERT INTO users (username, password_hash, email, favorite_genre)
                VALUES (@username, @password, @email, @genre)
                RETURNING id;
            ", connection);

            // admin user
            insertUserCmd.Parameters.AddWithValue("username", "admin");
            insertUserCmd.Parameters.AddWithValue("password", PasswordHasher.HashPassword("admin123"));
            insertUserCmd.Parameters.AddWithValue("email", DBNull.Value);
            insertUserCmd.Parameters.AddWithValue("genre", "SciFi");
            var adminId = (int)insertUserCmd.ExecuteScalar();

            // alice
            insertUserCmd.Parameters.Clear();
            insertUserCmd.Parameters.AddWithValue("username", "alice");
            insertUserCmd.Parameters.AddWithValue("password", PasswordHasher.HashPassword("alice123"));
            insertUserCmd.Parameters.AddWithValue("email", DBNull.Value);
            insertUserCmd.Parameters.AddWithValue("genre", "Fantasy");
            var aliceId = (int)insertUserCmd.ExecuteScalar();

            // bob
            insertUserCmd.Parameters.Clear();
            insertUserCmd.Parameters.AddWithValue("username", "bob");
            insertUserCmd.Parameters.AddWithValue("password", PasswordHasher.HashPassword("bob123"));
            insertUserCmd.Parameters.AddWithValue("email", DBNull.Value);
            insertUserCmd.Parameters.AddWithValue("genre", "Comedy");
            var bobId = (int)insertUserCmd.ExecuteScalar();

            // seed media entries + genres
            var mediaEntries = new List<(string Title, string Description, string Type, int Year, int Age, string[] Genres)>
            {
                ("Avatar: Fire and Ash", "Blue people fight for their planet", "Movie", 2025, 12, new [] { "animation", "adventure" }),
                ("Final Fantasy VII", "Fantasy RPG", "Game", 1997, 12, new [] { "fantasy", "adventure" }),
                ("Inception", "Mind-bending sci-fi thriller", "Movie", 2010, 12, new [] { "sci-fi", "thriller" }),
                ("The Witcher 3", "Monster hunting RPG", "Game", 2015, 18, new [] { "fantasy", "action" }),
                ("Stranger Things", "Kids face supernatural forces", "Series", 2016, 12, new [] { "sci-fi", "drama", "thriller" }),
                ("The Office", "Comedy series in an office", "Series", 2005, 12, new [] { "comedy" }),
                ("Interstellar", "Epic space journey", "Movie", 2014, 12, new [] { "sci-fi", "adventure" }),
                ("Minecraft", "Sandbox building game", "Game", 2011, 7, new [] { "adventure", "simulation" }),
                ("Joker", "Dark psychological drama", "Movie", 2019, 18, new [] { "drama", "thriller" }),
                ("Frozen", "Princesses in a magical kingdom", "Movie", 2013, 0, new [] { "animation", "adventure", "fantasy" })
            };

            var mediaIds = new List<int>();

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
                    new NpgsqlParameter("creator", adminId)
                }}
                .ExecuteScalar();

                mediaIds.Add(mediaId);

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

            // seed rating
            var ratings = new List<(int Stars, string Comment, int MediaIndex, int UserId)>
            {
                // Admin ratings
                (5, "Loved it!", 0, adminId),
                (4, "Pretty good", 1, adminId),
                (5, "Mind-blowing", 2, adminId),
                (3, "Not bad", 3, adminId),
                (4, "Interesting plot", 4, adminId),
                // Alice ratings
                (5, "Fantastic fantasy world", 1, aliceId),
                (4, "Epic adventure!", 3, aliceId),
                (3, "Good story", 9, aliceId),
                // Bob ratings
                (5, "So funny!", 5, bobId),
                (4, "Liked the comedy", 6, bobId),
                (5, "Hilarious moments", 0, bobId)
            };

            foreach (var rating in ratings)
            {
                using var ratingCmd = new NpgsqlCommand(@"
                    INSERT INTO ratings (stars, comment, is_confirmed, user_id, media_id)
                    VALUES (@stars, @comment, TRUE, @userId, @mediaId);
                ", connection);

                ratingCmd.Parameters.AddWithValue("stars", rating.Stars);
                ratingCmd.Parameters.AddWithValue("comment", rating.Comment);
                ratingCmd.Parameters.AddWithValue("userId", rating.UserId);
                ratingCmd.Parameters.AddWithValue("mediaId", mediaIds[rating.MediaIndex]);
                ratingCmd.ExecuteNonQuery();
            }

            // seed favorite
            var favoriteIndices = new[] { 0, 2, 4, 6 };
            foreach (var index in favoriteIndices)
            {
                using var favoriteCmd = new NpgsqlCommand(@"
                    INSERT INTO favorites (user_id, media_id)
                    VALUES (@userId, @mediaId)
                ", connection);

                favoriteCmd.Parameters.AddWithValue("userId", adminId);
                favoriteCmd.Parameters.AddWithValue("mediaId", mediaIds[index]);
                favoriteCmd.ExecuteNonQuery();
            }
        }
    }
}
