using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TriDict.EntityFrameworkCore;

public sealed class TriDictDbContextFactory : IDesignTimeDbContextFactory<TriDictDbContext>
{
    public TriDictDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("TRIDICT_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=tridict;Username=tridict;Password=tridict_dev";
        var builder = new DbContextOptionsBuilder<TriDictDbContext>();
        builder.UseNpgsql(connectionString);
        return new TriDictDbContext(builder.Options);
    }
}
