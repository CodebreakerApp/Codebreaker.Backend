using Codebreaker.GameAPIs.Data.Cosmos.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Codebreaker.GameAPIs.Data.Cosmos.Tests;
public class TestCosmosFixture
{
    // a test database (e.g. with the Cosmos enumlator), don't use the real one!
    private const string ConnectionString = @"AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
    private static readonly object s_lock = new();
    private static bool s_databaseInitialized;

    public TestCosmosFixture()
    {
        lock (s_lock)
        {
            if (!s_databaseInitialized)
            {
                using var context = CreateContext();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                s_databaseInitialized = true;

            }
        }
    }

    public CodebreakerCosmosContext CreateContext()
        => new(
            new DbContextOptionsBuilder<CodebreakerCosmosContext>()
            .UseCosmos<CodebreakerCosmosContext>(ConnectionString, "sampledb").Options, new NullLogger<CodebreakerCosmosContext>());

}

