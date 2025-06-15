// ViewModels/KategorieEditViewModel.cs
using Piwonka.CC.Models;
using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.ViewModels
{
    public class KategorieEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Der Name ist erforderlich.")]
        [StringLength(100, ErrorMessage = "Der Name darf maximal 100 Zeichen lang sein.")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Die Beschreibung darf maximal 500 Zeichen lang sein.")]
        public string? Beschreibung { get; set; }

        [Required(ErrorMessage = "Die Sprache ist erforderlich.")]
        public Language Language { get; set; } = Language.DE;
    }
}