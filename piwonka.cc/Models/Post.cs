using Piwonka.CC.Models;
using System.ComponentModel.DataAnnotations;

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

        public bool IstVeroeffentlicht { get; set; } = false;

		public virtual Language Language { get; set; } = Language.DE;


        
        // Navigation Property für Kategorie
        public virtual Kategorie? Kategorie { get; set; }


	        public string GetLanguageCode()
        {
            return Language switch
            {
                Language.DE => "de",
                Language.EN => "en",
                _ => "de"
            };
        }

    }
}