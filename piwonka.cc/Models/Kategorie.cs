using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.Models
{
    public class Kategorie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Beschreibung { get; set; }

        // Navigation Property für Posts
        public ICollection<Post>? Posts { get; set; }
    }
}
