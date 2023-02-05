using Microsoft.EntityFrameworkCore;

namespace CodeBreaker.Data.Users;

public class CodeBreakerUsersContext : DbContext
{
    public CodeBreakerUsersContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.ApplyConfiguration
    }
}
