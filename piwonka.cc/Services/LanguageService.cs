using Piwonka.CC.Models;

namespace Piwonka.CC.Services
{
	public class LanguageService 
	{
		public static Dictionary<Language, string> SupportedLanguages { get; } = new Dictionary<Language, string>
		{
			{ Language.DE, "Deutsch" },
			{ Language.EN, "English" }
		};

		public static Language GetLanguageFromString(string languageCode)
		{
			return languageCode?.ToUpper() switch
			{
				"DE" => Language.DE,
				"EN" => Language.EN,
				_ => Language.DE // Default
			};
		}

		public static string GetLanguageCode(Language language)
		{
			return language switch
			{
				Language.DE => "de",
				Language.EN => "en",
				_ => "de"
			};
		}

		public static string GetLanguageName(Language language)
		{
			return SupportedLanguages.TryGetValue(language, out var name) ? name : "Deutsch";
		}
	}
}