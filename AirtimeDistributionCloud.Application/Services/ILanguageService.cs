namespace AirtimeDistributionCloud.Application.Services;

public interface ILanguageService
{
    string CurrentLanguage { get; }
    bool IsRtl { get; }
    string T(string key);
    string T(string key, params object[] args);
    event Action? OnLanguageChanged;
    void SetLanguage(string lang);
}
