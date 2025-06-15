
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Piwonka.CC.Services
{
    public static class SlugGenerator
    {
        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            // Normalisieren (entfernt Akzente usw.)
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Zu Kleinbuchstaben, entferne nicht-alphanumerische Zeichen und ersetze Leerzeichen
            var slug = Regex.Replace(stringBuilder.ToString().ToLowerInvariant(), @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug;
        }
    }
}