using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TriDict.EntityFrameworkCore;
using TriDict.Terminology;

var builder = Host.CreateApplicationBuilder(args);
var connectionString = builder.Configuration["ConnectionStrings:Default"]
    ?? Environment.GetEnvironmentVariable("TRIDICT_CONNECTION_STRING")
    ?? "Host=localhost;Port=5432;Database=tridict;Username=tridict;Password=tridict_dev";

builder.Services.AddDbContext<TriDictDbContext>(options => options.UseNpgsql(connectionString));
using var host = builder.Build();
await using var scope = host.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetRequiredService<TriDictDbContext>();
await dbContext.Database.MigrateAsync();

if (!await dbContext.Domains.AnyAsync())
{
    dbContext.Domains.AddRange(
        new DomainCategory(Guid.Parse("10000000-0000-0000-0000-000000000001"), "ECON", "经贸", "economía y comercio", "economics and trade", "/ECON", 10),
        new DomainCategory(Guid.Parse("10000000-0000-0000-0000-000000000002"), "IT", "信息技术", "tecnologías de la información", "information technology", "/IT", 20),
        new DomainCategory(Guid.Parse("10000000-0000-0000-0000-000000000003"), "RES", "科研教育", "investigación y educación", "research and education", "/RES", 30),
        new DomainCategory(Guid.Parse("10000000-0000-0000-0000-000000000004"), "CUL", "文化交流", "intercambio cultural", "cultural exchange", "/CUL", 40));
    await dbContext.SaveChangesAsync();
}

Console.WriteLine("TriDict database migration and seed completed.");
