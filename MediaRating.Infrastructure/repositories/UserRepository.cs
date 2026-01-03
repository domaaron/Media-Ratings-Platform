using MediaRatings.Domain;
using MediaRatings.Domain.repositories;
using MediaRatings.Infrastructure.security;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace MediaRatings.Infrastructure.repositories
{
    /*
    Repository for managing user accounts in the database:
        - Provides CRUD operations for UserAccount objects
        - Methods:
            • FindByUsernameAsync(username) → retrieves a user by username, returns null if not found
            • CreateAsync(username, passwordPlain) → creates a new user with hashed password, throws if username exists
            • ValidateCredentialsAsync(username, passwordPlain) → verifies username/password combination
            • GetByIdAsync(userId) → retrieves a user by ID
            • UpdateProfileAsync(user) → updates email and favorite genre
            • GetAllUsersAsync() → retrieves all users
            • UpdatePasswordAsync(username, newPassword) → updates password (for exercises)
            • DeleteAsync(username) → deletes a user (for exercises)
        - Passwords are hashed via PasswordHasher
    */
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<UserAccount?> FindByUsernameAsync(string username)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new NpgsqlCommand(
                "SELECT id, username, password_hash, email, favorite_genre FROM users WHERE username = @u",
                connection
            );
            cmd.Parameters.AddWithValue("u", username ?? throw new ArgumentNullException(nameof(username)));

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            var user = new UserAccount(
                reader.GetString(1), // username
                reader.GetString(2), // password hash
                null!, null!, null!
            );

            // assign ID from database (int)
            typeof(UserAccount)
                .GetProperty(nameof(UserAccount.UserId))!
                .SetValue(user, reader.GetInt32(0));

            user.Email = reader.IsDBNull(3) ? null : reader.GetString(3);
            user.FavoriteGenre = reader.IsDBNull(4) ? null : reader.GetString(4);

            return user;
        }

        public async Task<UserAccount> CreateAsync(string username, string passwordPlain)
        {
            // hash password
            var passwordHash = PasswordHasher.HashPassword(passwordPlain);

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new NpgsqlCommand(
                "INSERT INTO users (username, password_hash) VALUES (@u, @p) RETURNING id",
                connection
            );
            cmd.Parameters.AddWithValue("u", username);
            cmd.Parameters.AddWithValue("p", passwordHash);

            int userId;
            try
            {
                userId = (int)await cmd.ExecuteScalarAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // unique violation
            {
                throw new InvalidOperationException("Username already exists.");
            }

            return new UserAccount(userId, username, passwordHash, null!, null!, null!);
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string passwordPlain)
        {
            var user = await FindByUsernameAsync(username);
            if (user is null)
            {
                return false;
            }

            return PasswordHasher.Verify(passwordPlain, user.Password);
        }

        public async Task<UserAccount?> GetByIdAsync(int userId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new NpgsqlCommand(
                "SELECT id, username, password_hash FROM users WHERE id = @id",
                connection
            );
            cmd.Parameters.AddWithValue("id", userId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            var user = new UserAccount(
                reader.GetString(1),
                reader.GetString(2),
                null!, null!, null!
            );

            // assign ID from database (int)
            typeof(UserAccount)
                .GetProperty(nameof(UserAccount.UserId))!
                .SetValue(user, reader.GetInt32(0));

            return user;
        }

        public async Task UpdateProfileAsync(UserAccount user)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new NpgsqlCommand(
                @"UPDATE users
                SET email = @e, favorite_genre = @g
                WHERE id = @id",
                connection
            );

            cmd.Parameters.AddWithValue("e", (object?)user.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("g", (object?)user.FavoriteGenre ?? DBNull.Value);
            cmd.Parameters.AddWithValue("id", user.UserId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<UserAccount>> GetAllUsersAsync()
        {
            var users = new List<UserAccount>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new NpgsqlCommand(
                "SELECT id, username, password_hash, email, favorite_genre FROM users",
                connection
            );

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var user = new UserAccount(
                    reader.GetString(1),    // username
                    reader.GetString(2),     // password hash
                    null!, null!, null!
                );

                // set ID
                typeof(UserAccount)
                    .GetProperty(nameof(UserAccount.UserId))!
                    .SetValue(user, reader.GetInt32(0));

                user.Email = reader.IsDBNull(3) ? null : reader.GetString(3);
                user.FavoriteGenre = reader.IsDBNull(4) ? null : reader.GetString(4);

                users.Add(user);
            }

            return users;
        }

        // not necessary for this project, only implemented for the database exercises
        public async Task UpdatePasswordAsync(string username, string newPassword)
        {
            var newHash = PasswordHasher.HashPassword(newPassword);
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new NpgsqlCommand(
                "UPDATE users SET password_hash = @p WHERE username = @u",
                connection
            );
            cmd.Parameters.AddWithValue("u", username);
            cmd.Parameters.AddWithValue("p", newHash);

            await cmd.ExecuteNonQueryAsync();
        }

        // not necessary for this project, only implemented for the database exercises
        public async Task DeleteAsync(string username)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new NpgsqlCommand(
                "DELETE FROM users WHERE username = @u",
                connection
            );
            cmd.Parameters.AddWithValue("u", username);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
