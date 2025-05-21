namespace SportSystem2.Models.DTOs
{
    public class JudgeDTO
    {
        public int JudgeId { get; set; }
        public string FullName { get; set; } = null!;
        public string QualificationLevel { get; set; } = null!;
        public DateTime? LastAttestationDate { get; set; }
        public string AvatarUrl { get; set; } = null!;
        public List<GameAssignment> GameAssignments { get; set; } = new();
        public List<MatchAnalysis> MatchAnalyses { get; set; } = new();
    }
}
