using Piwonka.CC.Models;
using Piwonka.CC.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piwonka.CC.Services
{
    public interface ISearchService
    {
        Task<SearchResultViewModel> SearchAsync(string query, Language? languageCode = null, int page = 1, int pageSize = 10);
        Task<List<string>> GetSearchSuggestionsAsync(string query, Language? languageCode = null, int maxResults = 5);
        Task IndexContentAsync(); // Für Suchindex-Aufbau
    }
}