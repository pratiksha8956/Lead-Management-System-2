using LeadManagement.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeadManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService auth, ILogger<AuthController> logger)
    {
        _auth = auth;
        _logger = logger;
    }

    public record LoginRequest(string Email, string Password);

    public record RegisterRequest(string Email, string Password, string DisplayName);

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.DisplayName))
            return BadRequest(new { message = "Email, password, and name are required." });

        try
        {
            await _auth.RegisterAsync(request.Email, request.Password, request.DisplayName, cancellationToken);
            return Ok(new { message = "Registration successful. You can sign in now." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required." });

        var result = await _auth.LoginAsync(request.Email, request.Password, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Failed login attempt for {Email}", request.Email);
            return Unauthorized(new { message = "Invalid credentials" });
        }

        return Ok(new
        {
            token = result.Token,
            user = new
            {
                userId = result.User.UserId,
                email = result.User.Email,
                name = result.User.Name,
                role = result.User.Role,
                salesRepId = result.User.SalesRepId
            }
        });
    }
}
