using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportSystem2.Models
{
    public class MatchResult
    {
        [Key]
        public int MatchResultId { get; set; }

        [ForeignKey(nameof(Match))]
        public int MatchId { get; set; }
        public Match Match { get; set; } = null!;

        [ForeignKey(nameof(Team))]
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public int Points { get; set; }
    }
}
