// ViewModels/KategorieEditViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.ViewModels
{
    public class KategorieEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Beschreibung { get; set; }
    }
}