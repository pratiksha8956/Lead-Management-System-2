using LeadManagement.Application.Abstractions;
using LeadManagement.Domain.Entities;
using LeadManagement.Domain.Enums;
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

    public async Task RegisterAsync(string email, string password, string displayName, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
            throw new InvalidOperationException("Email is required.");

        if (string.IsNullOrWhiteSpace(displayName))
            throw new InvalidOperationException("Name is required.");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new InvalidOperationException("Password must be at least 6 characters.");

        var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
        if (exists)
            throw new InvalidOperationException("An account with this email already exists.");

        var rep = new SalesRep
        {
            Name = displayName.Trim(),
            Email = normalizedEmail
        };
        _db.SalesReps.Add(rep);
        await _db.SaveChangesAsync(cancellationToken);

        var user = new AppUser
        {
            Email = normalizedEmail,
            DisplayName = displayName.Trim(),
            Role = UserRole.SalesRep,
            SalesRepId = rep.SalesRepId,
            PasswordHash = string.Empty
        };
        user.PasswordHash = _hasher.HashPassword(user, password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User registered: {Email} as SalesRep {SalesRepId}", normalizedEmail, rep.SalesRepId);
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
