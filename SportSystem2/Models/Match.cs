using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportSystem2.Models
{
    public class Match
    {
        [Key]
        public int MatchId { get; set; }

        public DateTime Date { get; set; }

        //[Required]
        //public string Location { get; set; } = null!;

        [ForeignKey(nameof(TeamA))]
        public int TeamAId { get; set; }
        [ValidateNever]
        public Team TeamA { get; set; } = null!;

        [ForeignKey(nameof(TeamB))]
        public int TeamBId { get; set; }
        [ValidateNever]
        public Team TeamB { get; set; } = null!;

        [ForeignKey(nameof(Tournament))]
        public int TournamentId { get; set; }
        [ValidateNever]
        public Tournament Tournament { get; set; } = null!;

        [ForeignKey(nameof(TournamentRound))]
        public int TournamentRoundId { get; set; }
        [ValidateNever]
        public TournamentRound TournamentRound { get; set; } = null!;

        //[ForeignKey(nameof(Judge))]
        //public int JudgeId { get; set; }
        //public Judge Judge { get; set; } = null!;

        public List<MatchResult> MatchResults { get; set; } = new();
        public List<PlayerEvent> PlayerEvents { get; set; } = new();
        public List<GameAssignment> GameAssignments { get; set; } = new();
        public List<MatchAnalysis> MatchAnalyses { get; set; } = new();
    }
}
