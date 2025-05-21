using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportSystem2.Models
{
    public enum JudgeRole
    {
        R,
        FJ,
        DJ,
        SJ
    }
    public class GameAssignment
    {
        [Key]
        public int GameAssignmentId { get; set; }

        [ForeignKey(nameof(Judge))]
        public int JudgeId { get; set; }
        [ValidateNever]
        public Judge Judge { get; set; } = null!;

        [ForeignKey(nameof(Match))]
        public int MatchId { get; set; }
        [ValidateNever]
        public Match Match { get; set; } = null!;

        [Required]
        public JudgeRole Role { get; set; }
    }
}
