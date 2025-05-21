using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportSystem2.Models
{
    public class MatchAnalysis
    {
        [Key]
        public int MatchAnalysisId { get; set; }

        [ForeignKey(nameof(Match))]
        public int MatchId { get; set; }
        [ValidateNever]
        public Match Match { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(5000, MinimumLength = 10)]
        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime? LastEditedAt { get; set; }

        [ForeignKey(nameof(CreatedByJudge))]
        public int CreatedByJudgeId { get; set; }
        [ValidateNever]
        public Judge CreatedByJudge { get; set; } = null!;

        public string? AttachmentUrl { get; set; }

        public TimeSpan? MinuteFrom { get; set; }
        public TimeSpan? MinuteTo { get; set; }
    }
}
