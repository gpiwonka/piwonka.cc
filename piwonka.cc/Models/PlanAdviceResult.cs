using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.Models
{
    public class PlanAdviceResult
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(16)]
        public string Hash { get; set; } = string.Empty;

        [Required]
        public string ExecutionPlan { get; set; } = string.Empty;

        [Required]
        public string AnalysisResult { get; set; } = string.Empty;

        [MaxLength(2)]
        public string Language { get; set; } = "de";

        public bool WasTruncated { get; set; }

        [MaxLength(30)]
        public string ToolType { get; set; } = "PlanAdvice";

        public DateTime ErstelltAm { get; set; } = DateTime.Now;

        public DateTime AblaufAm { get; set; } = DateTime.Now.AddDays(30);
    }
}
