using MediaRatings.Infrastructure.repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Media_Ratings_Platform.Test
{
    public class UserRepositoryTest : IAsyncLifetime
    {
        private readonly UserRepository _repository;
        private const string ConnectionString = "Host=localhost;Database=mrp;Username=postgres;Password=1234";

        public UserRepositoryTest()
        {
            _repository = new UserRepository(ConnectionString);
        }

        public async Task InitializeAsync()
        {
            await using var connection = new Npgsql.NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();
            var cmd = new Npgsql.NpgsqlCommand("DELETE FROM users;", connection);
            await cmd.ExecuteNonQueryAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task CreateUserSuccessTest()
        {
            var bob = await _repository.CreateAsync("Bob", "password");
            var findBob = await _repository.FindByUsernameAsync("Bob");

            var alice = await _repository.CreateAsync("Alice", "password");
            var findAlice = await _repository.FindByUsernameAsync("Alice");

            Assert.NotNull(findBob);
            Assert.NotNull(findAlice);
        }

        [Fact]
        public async Task UpdatePasswordSuccessTest()
        {
            await _repository.CreateAsync("Bob", "oldPassword");
            await _repository.UpdatePasswordAsync("Bob", "newPassword");

            await _repository.CreateAsync("Alice", "oldPassword");
            await _repository.UpdatePasswordAsync("Alice", "newPassword");

            var validBob = await _repository.ValidateCredentialsAsync("Bob", "newPassword");
            var validAlice = await _repository.ValidateCredentialsAsync("Alice", "newPassword");

            Assert.True(validBob);
            Assert.True(validAlice);
        }

        [Fact]
        public async Task DeleteUserSuccessTest()
        {
            await _repository.CreateAsync("Bob", "password");
            await _repository.DeleteAsync("Bob");

            await _repository.CreateAsync("Alice", "password");
            await _repository.DeleteAsync("Alice");

            var bob = await _repository.FindByUsernameAsync("Bob");
            var alice = await _repository.FindByUsernameAsync("Alice");

            Assert.Null(bob);
            Assert.Null(alice);
        }
    }
}
