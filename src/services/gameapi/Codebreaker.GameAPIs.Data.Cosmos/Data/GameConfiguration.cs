using Codebreaker.GameAPIs.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codebreaker.GameAPIs.Data.Cosmos.Data;

internal class GameConfiguration<T> : IEntityTypeConfiguration<T>
    where T : Game
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property<string>(ColumnNames.StartDate);

        builder.HasPartitionKey(ColumnNames.StartDate);
    }
}
