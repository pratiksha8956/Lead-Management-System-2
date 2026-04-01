using LeadManagement.Application.Abstractions;
using LeadManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LeadManagement.Infrastructure.Persistence;

namespace LeadManagement.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly PasswordHasher<AppUser> _hasher;
    private readonly JwtTokenService _jwt;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext db,
        PasswordHasher<AppUser> hasher,
        JwtTokenService jwt,
        ILogger<AuthService> logger)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
            u => u.Email.ToLower() == email.Trim().ToLowerInvariant(),
            cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Login failed for {Email}: user not found", email);
            return null;
        }

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verify == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Login failed for {Email}: bad password", email);
            return null;
        }

        var token = _jwt.CreateToken(user);
        return new LoginResponse
        {
            Token = token,
            User = new LoginUserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.DisplayName,
                Role = user.Role.ToString(),
                SalesRepId = user.SalesRepId
            }
        };
    }
}
