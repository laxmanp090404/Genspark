using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using server.Repositories.Interfaces;

namespace server.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), ct);

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default)
        => db.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower(), ct);

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
        => db.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower(), ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await db.Users.AddAsync(user, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<User> GetSingleAdminAsync(CancellationToken ct = default)
    {
        var admin = await db.Users.SingleOrDefaultAsync(u => u.Role == UserRole.ADMIN, ct);
        return admin ?? throw new InvalidOperationException("No admin configured in the system.");
    }
}
