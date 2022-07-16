using CodeBreaker.Shared.Models.Data;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBreaker.APIs.Data.DbConfiguration;

public class GameDbConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(x => x.GameId);
        builder.HasPartitionKey(x => x.GameId);
        //builder.OwnsOne(x => x.Type);
        //builder.OwnsMany(
        //    x => x.Moves,
        //    x => x.ToJsonProperty("Moves")
        //);
    }
}
