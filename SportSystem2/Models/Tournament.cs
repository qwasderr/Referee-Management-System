using System.ComponentModel.DataAnnotations;

namespace SportSystem2.Models
{
    public enum TournamentType
    {
        GroupStage,
        Knockout
    }
    public class Tournament
    {
        [Key]
        public int TournamentId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public TournamentType Type { get; set; }

        public ICollection<TournamentRound> Rounds { get; set; } = new List<TournamentRound>();
        public List<Match> Matches { get; set; } = new();
    }

}
