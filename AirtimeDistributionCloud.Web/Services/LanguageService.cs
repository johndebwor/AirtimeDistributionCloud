using AirtimeDistributionCloud.Application.Services;
using AirtimeDistributionCloud.Web.Localization;

namespace AirtimeDistributionCloud.Web.Services;

public class LanguageService : ILanguageService
{
    private string _currentLanguage = "en";

    public string CurrentLanguage => _currentLanguage;
    public bool IsRtl => _currentLanguage == "ar";

    public event Action? OnLanguageChanged;

    public string T(string key) => Translations.Get(key, _currentLanguage);

    public string T(string key, params object[] args)
    {
        var template = Translations.Get(key, _currentLanguage);
        return args.Length > 0 ? string.Format(template, args) : template;
    }

    public void SetLanguage(string lang)
    {
        if (lang != "en" && lang != "ar") lang = "en";
        if (_currentLanguage == lang) return;
        _currentLanguage = lang;
        OnLanguageChanged?.Invoke();
    }
}
