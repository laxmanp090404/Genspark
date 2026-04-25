using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace server.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var envConnection = Environment.GetEnvironmentVariable("DATABASE_URL");
        var connectionString = string.IsNullOrWhiteSpace(envConnection)
            ? "Host=localhost;Port=5432;Database=bus_management_dev;Username=laxmanp;"
            : envConnection;

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
