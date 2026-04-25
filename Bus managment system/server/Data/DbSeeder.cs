using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var db = services.GetRequiredService<AppDbContext>();

        var adminExists = await db.Users.AnyAsync(u => u.Role == UserRole.ADMIN);
        if (adminExists)
        {
            return;
        }

        var username = configuration["ADMIN_SEED_USERNAME"];
        var email = configuration["ADMIN_SEED_EMAIL"];
        var password = configuration["ADMIN_SEED_PASSWORD"];

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var admin = new User
        {
            UserId = Guid.NewGuid(),
            Username = username.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Gender = Gender.OTHER,
            Age = 30,
            Role = UserRole.ADMIN,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}
