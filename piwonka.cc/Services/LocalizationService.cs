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
            ["NextPost"] = new() { [Language.DE] = "Nächster Beitrag", [Language.EN] = "Next Post" },

            // Neue Strings für Social Media
            ["ShareOnFacebook"] = new() { [Language.DE] = "Auf Facebook teilen", [Language.EN] = "Share on Facebook" },
            ["ShareOnTwitter"] = new() { [Language.DE] = "Auf Twitter teilen", [Language.EN] = "Share on Twitter" },
            ["ShareOnLinkedIn"] = new() { [Language.DE] = "Auf LinkedIn teilen", [Language.EN] = "Share on LinkedIn" },
            ["ShareViaEmail"] = new() { [Language.DE] = "Per E-Mail teilen", [Language.EN] = "Share via Email" },

            // Navigation
            ["FirstPage"] = new() { [Language.DE] = "Erste Seite", [Language.EN] = "First page" },
            ["LastPage"] = new() { [Language.DE] = "Letzte Seite", [Language.EN] = "Last page" },
            ["PreviousPage"] = new() { [Language.DE] = "Vorherige Seite", [Language.EN] = "Previous page" },
            ["NextPage"] = new() { [Language.DE] = "Nächste Seite", [Language.EN] = "Next page" },
            ["PageNavigation"] = new() { [Language.DE] = "Seitennavigation", [Language.EN] = "Page navigation" },
            ["Page"] = new() { [Language.DE] = "Seite", [Language.EN] = "Page" },
            ["Of"] = new() { [Language.DE] = "von", [Language.EN] = "of" },

            // Common phrases
            ["DiscoverLatestPosts"] = new() { [Language.DE] = "Entdecken Sie unsere neuesten Beiträge", [Language.EN] = "Discover our latest posts" },
            ["PostNotFound"] = new() { [Language.DE] = "Beitrag nicht gefunden", [Language.EN] = "Post not found" },
            ["BackToBlog"] = new() { [Language.DE] = "Zurück zum Blog", [Language.EN] = "Back to Blog" },
            ["ReadingTime"] = new() { [Language.DE] = "Lesezeit", [Language.EN] = "Reading time" },
            ["Minutes"] = new() { [Language.DE] = "Minuten", [Language.EN] = "minutes" },

            // Error messages
            ["ErrorOccurred"] = new() { [Language.DE] = "Ein Fehler ist aufgetreten", [Language.EN] = "An error occurred" },
            ["LoadingError"] = new() { [Language.DE] = "Fehler beim Laden", [Language.EN] = "Loading error" },

            // Search and filters
            ["Search"] = new() { [Language.DE] = "Suchen", [Language.EN] = "Search" },
            ["Filter"] = new() { [Language.DE] = "Filter", [Language.EN] = "Filter" },
            ["SortBy"] = new() { [Language.DE] = "Sortieren nach", [Language.EN] = "Sort by" },
            ["Date"] = new() { [Language.DE] = "Datum", [Language.EN] = "Date" },
            ["Newest"] = new() { [Language.DE] = "Neueste", [Language.EN] = "Newest" },
            ["Oldest"] = new() { [Language.DE] = "Älteste", [Language.EN] = "Oldest" }
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