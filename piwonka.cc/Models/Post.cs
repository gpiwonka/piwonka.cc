using Piwonka.CC.Models;
using Piwonka.CC.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Piwonka.CC.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Titel { get; set; }


		[Required]
		public string Excerpt { get; set; }

        [StringLength(500)]
        public string? MetaDescription { get; set; }
        [StringLength(500)]
        public string? MetaKeywords { get; set; }
        [DataType(DataType.Html)]   

		[Required]
        public string Inhalt { get; set; }

        [StringLength(100)]
        public string Slug { get; set; }

        [DataType(DataType.Date)]
        public DateTime ErstelltAm { get; set; } = DateTime.Now;

        public string? BildUrl { get; set; }

        public bool IstVeroeffentlicht { get; set; } = false;

		public virtual Language Language { get; set; } = Language.DE;


        
        // Navigation Property für Kategorie
        public virtual Kategorie? Kategorie { get; set; }


		[NotMapped]
        [OptionalFile]
        public IFormFile UploadedImage { get; set; } = default!;
    }
}