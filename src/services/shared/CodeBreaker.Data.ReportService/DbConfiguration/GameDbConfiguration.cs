using CodeBreaker.Data.ReportService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeBreaker.Data.ReportService.DbConfiguration;

internal class GameDbConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasPartitionKey(x => x.Id);
    }
}
