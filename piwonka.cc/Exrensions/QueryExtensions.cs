using Piwonka.CC.Models;

namespace Piwonka.CC.Exrensions
{
    namespace Piwonka.CC.Extensions
    {
        public static class QueryExtensions
        {
            public static IQueryable<Post> ByLanguage(this IQueryable<Post> query, string language = "de")
            {
                return query.Where(p => p.Language == language);
            }

            public static IQueryable<Kategorie> ByLanguage(this IQueryable<Kategorie> query, string language = "de")
            {
                return query.Where(k => k.Language == language);
            }
        }
    }
}
