using Piwonka.CC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piwonka.CC.Services
{
    public interface ILanguageService
    {
        Task<List<Language>> GetActiveLanguagesAsync();
        
        Task<Language> GetDefaultLanguageAsync();
        Task<Language> GetCurrentLanguageCodeAsync();
        Task SetCurrentLanguageAsync(string languageCode);
    }
}