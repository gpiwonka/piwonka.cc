using Piwonka.CC.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.ViewModels
{
    public class SearchResultViewModel
    {
        public string Query { get; set; } = string.Empty;
        public List<SearchResultItemViewModel> Results { get; set; } = new();
        public int TotalResults { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public Language LanguageCode { get; set; } 
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class SearchResultItemViewModel
    {
        public string Type { get; set; } = string.Empty; // "Seite", "Blog-Post", "Kategorie"
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int Relevance { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SearchFormViewModel
    {
        [Required(ErrorMessage = "Suchbegriff ist erforderlich")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Suchbegriff muss zwischen 2 und 100 Zeichen lang sein")]
        public string Query { get; set; } = string.Empty;

        public Language LanguageCode { get; set; } = default!;
    }
}

