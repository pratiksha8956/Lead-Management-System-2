using System.Security.Claims;
using LeadManagement.Application.Abstractions;
using LeadManagement.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace LeadManagement.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
    {
        _http = http;
    }

    private ClaimsPrincipal? User => _http.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public int UserId =>
        int.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public UserRole Role =>
        Enum.TryParse<UserRole>(User?.FindFirstValue(ClaimTypes.Role), true, out var r)
            ? r
            : UserRole.SalesRep;

    public int? SalesRepId
    {
        get
        {
            var raw = User?.FindFirstValue("sales_rep_id");
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
