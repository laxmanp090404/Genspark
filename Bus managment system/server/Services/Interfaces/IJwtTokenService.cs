using server.Models;

namespace server.Services.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) Generate(User user);
}
