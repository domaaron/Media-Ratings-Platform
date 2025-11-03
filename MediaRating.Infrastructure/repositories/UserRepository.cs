using MediaRatings.Domain;
using MediaRatings.Domain.repositories;
using MediaRatings.Infrastructure.security;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace MediaRatings.Infrastructure.repositories
{
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
                "SELECT id, username, password_hash FROM users WHERE username = @u",
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
