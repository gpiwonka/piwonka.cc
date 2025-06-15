using Piwonka.CC.Models;

namespace Piwonka.CC.Services
{
    public interface ILanguageService
    {
        Task<List<Language>> GetActiveLanguagesAsync();
        Task<Language> GetDefaultLanguageAsync();
        Task<Language> GetCurrentLanguageAsync();
        Task SetCurrentLanguageAsync(string languageCode);
        string GetLanguageCode(Language language);
        string GetLanguageName(Language language);
        Language GetLanguageFromString(string languageCode);
    }
}