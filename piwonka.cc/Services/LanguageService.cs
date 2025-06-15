using Microsoft.AspNetCore.Http;
using Piwonka.CC.Models;

namespace Piwonka.CC.Services
{
	public class LanguageService : ILanguageService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private const string LANGUAGE_SESSION_KEY = "CurrentLanguage";

		public static Dictionary<Language, string> SupportedLanguages { get; } = new Dictionary<Language, string>
		{
			{ Language.DE, "Deutsch" },
			{ Language.EN, "English" }
		};

		public LanguageService(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<List<Language>> GetActiveLanguagesAsync()
		{
			// Alle unterstützten Sprachen zurückgeben
			return await Task.FromResult(SupportedLanguages.Keys.ToList());
		}

		public async Task<Language> GetDefaultLanguageAsync()
		{
			return await Task.FromResult(Language.DE);
		}

		public async Task<Language> GetCurrentLanguageAsync()
		{
			var session = _httpContextAccessor.HttpContext?.Session;
			var languageCode = session?.GetString(LANGUAGE_SESSION_KEY);

			if (!string.IsNullOrEmpty(languageCode))
			{
				return await Task.FromResult(GetLanguageFromString(languageCode));
			}

			// Fallback auf Browser-Sprache oder Default
			var httpContext = _httpContextAccessor.HttpContext;
			if (httpContext?.Request.Headers.ContainsKey("Accept-Language") == true)
			{
				var acceptLanguage = httpContext.Request.Headers["Accept-Language"].ToString();
				if (acceptLanguage.StartsWith("en", StringComparison.OrdinalIgnoreCase))
				{
					return await Task.FromResult(Language.EN);
				}
			}

			return await GetDefaultLanguageAsync();
		}

		public async Task SetCurrentLanguageAsync(string languageCode)
		{
			var session = _httpContextAccessor.HttpContext?.Session;
			if (session != null)
			{
				session.SetString(LANGUAGE_SESSION_KEY, languageCode.ToUpper());
			}
			await Task.CompletedTask;
		}

		public Language GetLanguageFromString(string languageCode)
		{
			return languageCode?.ToUpper() switch
			{
				"DE" => Language.DE,
				"EN" => Language.EN,
				_ => Language.DE // Default
			};
		}

		public string GetLanguageCode(Language language)
		{
			return language switch
			{
				Language.DE => "de",
				Language.EN => "en",
				_ => "de"
			};
		}

		// Neue Methode: Gibt den Language Code für die aktuelle Sprache zurück
		public async Task<string> GetCurrentLanguageCodeAsync()
		{
			var currentLanguage = await GetCurrentLanguageAsync();
			return GetLanguageCode(currentLanguage);
		}

		// Statische Version für direkten Zugriff
		public static string GetLanguageCodeStatic(Language language)
		{
			return language switch
			{
				Language.DE => "de",
				Language.EN => "en",
				_ => "de"
			};
		}

		public string GetLanguageName(Language language)
		{
			return SupportedLanguages.TryGetValue(language, out var name) ? name : "Deutsch";
		}
	}
}