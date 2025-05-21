
using SportSystem2.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class TeamStanding
{
    [Key]
    public int TeamStandingId { get; set; }

    public int Position { get; set; }

    [ForeignKey(nameof(TeamId))]
    public int TeamId { get; set; }

    public Team Team { get; set; } = null!;
    [ForeignKey(nameof(TournamentId))]
    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }

    public int Points { get; set; }
    public int Scored { get; set; }
    public int Conceded { get; set; }

    [NotMapped]
    public int Difference => Scored - Conceded;
}
