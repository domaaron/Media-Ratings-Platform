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

            CreateUserTable(connection);
            CreateMediaTable(connection);
            CreateRatingsTable(connection);
            CreateFavoritesTable(connection);

            SeedInitialData(connection);
        }

        private void CreateUserTable(NpgsqlConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS users (
                    id SERIAL PRIMARY KEY,
                    username TEXT UNIQUE NOT NULL,
                    password_hash TEXT NOT NULL,
                    email TEXT,
                    favorite_genre TEXT
                );";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        private void CreateMediaTable(NpgsqlConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS media (
                    id SERIAL PRIMARY KEY,
                    title TEXT NOT NULL,
                    description TEXT,
                    media_type TEXT NOT NULL,
                    release_year INT,
                    age_restriction INT,
                    creator_user_id INT REFERENCES users(id) ON DELETE CASCADE
                );";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        private void CreateRatingsTable(NpgsqlConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS ratings (
                    id SERIAL PRIMARY KEY,
                    stars INT CHECK (stars BETWEEN 1 AND 5),
                    comment TEXT,
                    is_confirmed BOOLEAN DEFAULT FALSE,
                    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                    user_id INT REFERENCES users(id) ON DELETE CASCADE,
                    media_id INT REFERENCES media(id) ON DELETE CASCADE
                );";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        private void CreateFavoritesTable(NpgsqlConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS favorites (
                    user_id INT REFERENCES users(id) ON DELETE CASCADE,
                    media_id INT REFERENCES media(id) ON DELETE CASCADE,
                    PRIMARY KEY (user_id, media_id)
                );";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
        }

        private void SeedInitialData(NpgsqlConnection connection)
        {
            var checkCmd = new NpgsqlCommand("SELECT COUNT(*) from users;", connection);
            var userCount = (long)checkCmd.ExecuteScalar();

            if (userCount > 0)
            {
                return;
            }

            var insertUserSql = @"
                INSERT INTO users (username, password_hash, email, favorite_genre)
                VALUES (@username, @password, @email, @genre);";

            using var insertCmd = new NpgsqlCommand(insertUserSql, connection);
            insertCmd.Parameters.AddWithValue("username", "admin");
            insertCmd.Parameters.AddWithValue("password", PasswordHasher.HashPassword("admin123"));
            insertCmd.Parameters.AddWithValue("email", DBNull.Value);
            insertCmd.Parameters.AddWithValue("favorite_genre", DBNull.Value);
            insertCmd.ExecuteNonQuery();

            var insertMediaSql = @"
                INSERT INTO media (title, description, media_type, release_year, age_restriction, creator_user_id)
                VALUES 
                    ('Avatar: Fire and Ash', 'blue people', 'Movie', 2025, 12, 1),
                    ('Fínal Fantasy VII', 'Fantasy RPG', 'Game', 1997, 12, 1);";

            using var mediaCmd = new NpgsqlCommand(insertMediaSql, connection);
            mediaCmd.ExecuteNonQuery();
        }
    }
}
