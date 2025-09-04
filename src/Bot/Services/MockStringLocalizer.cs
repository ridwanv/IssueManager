using Microsoft.Extensions.Localization;
using System.Globalization;

namespace IssueManager.Bot.Services
{
    public class MockStringLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => new LocalizedString(name, name);

        public LocalizedString this[string name, params object[] arguments] => new LocalizedString(name, string.Format(name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Enumerable.Empty<LocalizedString>();
    }

    public class MockStringLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name] => new LocalizedString(name, name);

        public LocalizedString this[string name, params object[] arguments] => new LocalizedString(name, string.Format(name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Enumerable.Empty<LocalizedString>();
    }

    public class MockStringLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource) => new MockStringLocalizer();

        public IStringLocalizer Create(string baseName, string location) => new MockStringLocalizer();
    }
}