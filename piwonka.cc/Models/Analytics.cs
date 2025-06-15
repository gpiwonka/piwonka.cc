using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.Models
{
    public class Analytics
    {
        public int Id { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        public int UniqueVisitors { get; set; }

        public int PageViews { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class UserSession
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string SessionId { get; set; } = string.Empty;

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public DateTime FirstVisit { get; set; } = DateTime.Now;

        public DateTime LastActivity { get; set; } = DateTime.Now;

        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }
}
