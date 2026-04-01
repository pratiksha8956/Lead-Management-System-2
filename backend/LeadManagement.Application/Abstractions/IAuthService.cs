using LeadManagement.Application.Contracts;

namespace LeadManagement.Application.Abstractions;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}

public class LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public LoginUserDto User { get; init; } = null!;
}

public class LoginUserDto
{
    public int UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public int? SalesRepId { get; init; }
}
