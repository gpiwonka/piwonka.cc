using Microsoft.EntityFrameworkCore;
using Piwonka.CC.Data;
using Piwonka.CC.Interfaces;
using Piwonka.CC.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piwonka.CC.Services
{
	public class LanguageService : ILanguageService
	{
		private readonly ApplicationDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private const string LANGUAGE_SESSION_KEY = "CurrentLanguage";

		public LanguageService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
		{
			_context = context;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<List<Language>> GetActiveLanguagesAsync()
		{
			return await _context.Languages
				.Where(l => l.IsActive)
				.OrderBy(l => l.SortOrder)
				.ThenBy(l => l.Name)
				.ToListAsync();
		}

		public async Task<Language?> GetLanguageByCodeAsync(string code)
		{
			return await _context.Languages
				.FirstOrDefaultAsync(l => l.Code == code && l.IsActive);
		}

		public async Task<Language> GetDefaultLanguageAsync()
		{
			var defaultLang = await _context.Languages
				.FirstOrDefaultAsync(l => l.IsDefault && l.IsActive);

			if (defaultLang == null)
			{
				defaultLang = await _context.Languages
					.FirstOrDefaultAsync(l => l.IsActive);
			}

			return defaultLang ?? new Language { Code = "de", Name = "Deutsch" };
		}

		public async Task<string> GetCurrentLanguageCodeAsync()
		{
			var session = _httpContextAccessor.HttpContext?.Session;
			var currentLang = session?.GetString(LANGUAGE_SESSION_KEY);

			if (string.IsNullOrEmpty(currentLang))
			{
				var defaultLang = await GetDefaultLanguageAsync();
				currentLang = defaultLang.Code;
				await SetCurrentLanguageAsync(currentLang);
			}

			return currentLang;
		}

		public async Task SetCurrentLanguageAsync(string languageCode)
		{
			var session = _httpContextAccessor.HttpContext?.Session;
			if (session != null)
			{
				session.SetString(LANGUAGE_SESSION_KEY, languageCode);
			}
		}
	}
}