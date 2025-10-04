using MediaRatings.Domain;
using MediaRatings.Domain.repositories;
using MediaRatings.Infrastructure.security;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Infrastructure.repositories
{
    public class UserRepository : IUserRepository
    {
        // connection string for PostgreSQL database
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<UserAccount> FindByUsernameAsync(string username)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();


            // SQL command to select the user
            var cmd = new NpgsqlCommand(
                "SELECT id, username, password_hash FROM users WHERE username = @u",
                connection
                );
            // bind parameter safely to prevent SQL injection
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

            // set the ID from the database using reflection (private setter)
            typeof(UserAccount)
                .GetProperty(nameof(UserAccount.UserId))!
                .SetValue(user, reader.GetGuid(0));

            return user;
        }

        public async Task<UserAccount> CreateAsync(string username, string passwordPlain)
        {
            // hash the password before storing it
            var passwordHash = PasswordHasher.HashPassword(passwordPlain);
            var userId = Guid.NewGuid();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new NpgsqlCommand(
                "INSERT INTO users (id, username, password_hash) VALUES (@id, @u, @p)",
                connection
            );

            // bind parameters safely
            cmd.Parameters.AddWithValue("id", userId);
            cmd.Parameters.AddWithValue("u", username);
            cmd.Parameters.AddWithValue("p", passwordHash);

            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")  // unique violation (username already exists)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            var user = new UserAccount(username, passwordHash, null!, null!, null!);
            typeof(UserAccount)
                .GetProperty(nameof(UserAccount.UserId))!
                .SetValue(user, userId);

            return user;
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
    }
}
