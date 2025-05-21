using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportSystem2.Models
{
    public class Judge
    {
        [Key]
        public int JudgeId { get; set; }

        [ForeignKey(nameof(ApplicationUser))]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string QualificationLevel { get; set; } = null!;

        public DateTime? LastAttestationDate { get; set; }

        public string AvatarUrl => ApplicationUser?.PhotoUrl ?? "/images/default-avatar.png";

        public List<TestResult> TestResults { get; set; } = new();
        public List<Match> Matches { get; set; } = new();
        public List<GameAssignment> GameAssignments { get; set; } = new();
        public List<MatchAnalysis> MatchAnalyses { get; set; } = new();
    }
}
