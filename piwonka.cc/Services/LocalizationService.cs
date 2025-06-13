using Piwonka.CC.Models;

namespace Piwonka.CC.Services
{
    public interface ILocalizationService
    {
        Task<string> GetLocalizedStringAsync(string key, Language language);
        Task<string> GetLocalizedStringAsync(string key); // Verwendet aktuelle Sprache
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly ILanguageService _languageService;

        // Statisches Dictionary für Übersetzungen
        private static readonly Dictionary<string, Dictionary<Language, string>> Translations = new()
        {
            ["Home"] = new() { [Language.DE] = "Startseite", [Language.EN] = "Home" },
            ["Blog"] = new() { [Language.DE] = "Blog", [Language.EN] = "Blog" },
            ["Admin"] = new() { [Language.DE] = "Admin", [Language.EN] = "Admin" },
            ["Categories"] = new() { [Language.DE] = "Kategorien", [Language.EN] = "Categories" },
            ["Posts"] = new() { [Language.DE] = "Beiträge", [Language.EN] = "Posts" },
            ["Title"] = new() { [Language.DE] = "Titel", [Language.EN] = "Title" },
            ["Content"] = new() { [Language.DE] = "Inhalt", [Language.EN] = "Content" },
            ["Published"] = new() { [Language.DE] = "Veröffentlicht", [Language.EN] = "Published" },
            ["Draft"] = new() { [Language.DE] = "Entwurf", [Language.EN] = "Draft" },
            ["Create"] = new() { [Language.DE] = "Erstellen", [Language.EN] = "Create" },
            ["Edit"] = new() { [Language.DE] = "Bearbeiten", [Language.EN] = "Edit" },
            ["Delete"] = new() { [Language.DE] = "Löschen", [Language.EN] = "Delete" },
            ["Save"] = new() { [Language.DE] = "Speichern", [Language.EN] = "Save" },
            ["Cancel"] = new() { [Language.DE] = "Abbrechen", [Language.EN] = "Cancel" },
            ["ReadMore"] = new() { [Language.DE] = "Weiterlesen", [Language.EN] = "Read More" },
            ["AllPosts"] = new() { [Language.DE] = "Alle Beiträge", [Language.EN] = "All Posts" },
            ["NoPostsFound"] = new() { [Language.DE] = "Noch keine Beiträge vorhanden.", [Language.EN] = "No posts available yet." },
            ["RecentPosts"] = new() { [Language.DE] = "Neueste Beiträge", [Language.EN] = "Recent Posts" },
            ["RelatedPosts"] = new() { [Language.DE] = "Das könnte Sie auch interessieren", [Language.EN] = "You might also be interested in" },
            ["SharePost"] = new() { [Language.DE] = "Teilen Sie diesen Beitrag", [Language.EN] = "Share this post" },
            ["PreviousPost"] = new() { [Language.DE] = "Vorheriger Beitrag", [Language.EN] = "Previous Post" },
            ["NextPost"] = new() { [Language.DE] = "Nächster Beitrag", [Language.EN] = "Next Post" }
        };

        public LocalizationService(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        public async Task<string> GetLocalizedStringAsync(string key, Language language)
        {
            if (Translations.TryGetValue(key, out var translations) &&
                translations.TryGetValue(language, out var translation))
            {
                return await Task.FromResult(translation);
            }

            // Fallback auf Deutsch, dann auf den Key selbst
            if (Translations.TryGetValue(key, out var fallbackTranslations) &&
                fallbackTranslations.TryGetValue(Language.DE, out var fallbackTranslation))
            {
                return await Task.FromResult(fallbackTranslation);
            }

            return await Task.FromResult(key); // Wenn keine Übersetzung gefunden wird
        }

        public async Task<string> GetLocalizedStringAsync(string key)
        {
            var currentLanguage = await _languageService.GetCurrentLanguageAsync();
            return await GetLocalizedStringAsync(key, currentLanguage);
        }
    }
}