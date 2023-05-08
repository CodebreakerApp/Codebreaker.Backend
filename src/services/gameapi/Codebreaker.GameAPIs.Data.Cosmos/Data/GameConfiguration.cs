using Codebreaker.GameAPIs.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codebreaker.GameAPIs.Data.Cosmos.Data;
internal class GameConfiguration<T> : IEntityTypeConfiguration<T>
    where T : Game
{
    private const string StartDate = nameof(StartDate);

    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property<string>(StartDate);

        builder.HasPartitionKey(StartDate);
    }
}
