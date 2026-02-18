using System.Security.Claims;
using AirtimeDistributionCloud.Core.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace AirtimeDistributionCloud.Web.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private ClaimsPrincipal? _user;

    public CurrentUserService(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    public string? UserId => GetUser()?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? UserName => GetUser()?.Identity?.Name;
    public bool IsAuthenticated => GetUser()?.Identity?.IsAuthenticated ?? false;
    public bool IsInRole(string role) => GetUser()?.IsInRole(role) ?? false;

    private ClaimsPrincipal? GetUser()
    {
        if (_user is null)
        {
            var task = _authStateProvider.GetAuthenticationStateAsync();
            if (task.IsCompleted)
            {
                _user = task.Result.User;
            }
        }
        return _user;
    }
}
