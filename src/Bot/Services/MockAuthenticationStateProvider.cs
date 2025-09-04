using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace IssueManager.Bot.Services
{
    public class MockAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Create a mock system user identity for the bot
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "WhatsAppBot"),
                new Claim(ClaimTypes.Role, "System"),
                new Claim("UserId", "system-bot"),
                new Claim("TenantId", "default")
            }, "mock");

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
    }
}