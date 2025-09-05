using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Globalization;

namespace IssueManager.Bot.Services
{
    /// <summary>
    /// Mock string localizer factory for Bot project
    /// </summary>
    public class MockStringLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource)
        {
            return new MockStringLocalizer();
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return new MockStringLocalizer();
        }
    }

    /// <summary>
    /// Mock string localizer for Bot project
    /// </summary>
    public class MockStringLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return Enumerable.Empty<LocalizedString>();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return this;
        }
    }

    /// <summary>
    /// Mock string localizer generic for Bot project
    /// </summary>
    public class MockStringLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return Enumerable.Empty<LocalizedString>();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return this;
        }
    }

    /// <summary>
    /// Mock authentication state provider for Bot project
    /// </summary>
    public class MockAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var anonymous = new ClaimsIdentity();
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonymous)));
        }
    }
}