using MediaRatings.Infrastructure.repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Media_Ratings_Platform.Test
{
    public class DatabaseTest
    {
        private const string ConnectionString = "Host=localhost;Database=mrp;Username=postgres;Password=1234";

        [Fact]
        public async Task InvalidConnectionSuccessTest()
        {
            var badRepository = new UserRepository("Host=localhost;Database=mrp;Username=postgres;Password=wrongPassword");
            var exception = await Assert.ThrowsAsync<Npgsql.PostgresException>(async () =>
            {
                await badRepository.FindByUsernameAsync("Bob");
            });

            Console.WriteLine("Type: " + exception.GetType());
            Console.WriteLine("Message: " + exception.Message);
        }

        [Fact]
        public async Task InvalidQuerySuccessTest()
        {
            await using var connection = new Npgsql.NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            var cmd = new Npgsql.NpgsqlCommand("SELECT * FROM non_existing_table;", connection);
            var exception = await Assert.ThrowsAsync<Npgsql.PostgresException>(async () =>
            {
                await cmd.ExecuteReaderAsync();
            });

            Console.WriteLine("Type: " + exception.GetType());
            Console.WriteLine("Message: " + exception.Message);
        }
    }
}
