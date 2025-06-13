using Piwonka.CC.Models;

namespace Piwonka.CC.Interfaces
{
    public interface ILanguageService
    {
        Task<List<Language>> GetActiveLanguagesAsync();
        Task<Language?> GetLanguageByCodeAsync(string code);
        Task<Language> GetDefaultLanguageAsync();
        Task<string> GetCurrentLanguageCodeAsync();
        Task SetCurrentLanguageAsync(string languageCode);
    }
}
