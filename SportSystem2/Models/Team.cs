using System.ComponentModel.DataAnnotations;

namespace SportSystem2.Models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string City { get; set; } = null!;

        public List<Player> Players { get; set; } = new();
        public List<MatchResult> MatchResults { get; set; } = new();

        public string? PhotoPath { get; set; }
    }
}
