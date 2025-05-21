using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportSystem2.Models
{
    public enum PeriodType
    {
        Half = 1,
        Quarter = 2
    }

    public enum EventType
    {
        Touchdown = 1,
        Interception = 2,
        FlagsPulled = 3,
        Sack = 4,
        Fumble = 5,
        Safety = 6,
        FieldGoal = 7,
        ExtraPoint = 8,
        Kickoff = 9
    }
    public class PlayerEvent
    {
        [Key]
        public int PlayerEventId { get; set; }

        [ForeignKey(nameof(Player))]
        public int PlayerId { get; set; }
        [ValidateNever]
        public Player Player { get; set; } = null!;

        [ForeignKey(nameof(Match))]
        public int MatchId { get; set; }
        [ValidateNever]
        public Match Match { get; set; } = null!;

        [Required]
        public EventType EventType { get; set; }

        public int Minute { get; set; }

        public int? Yards { get; set; }

        public int? Points { get; set; }

        public int PeriodNumber { get; set; }

        public PeriodType PeriodType { get; set; } = PeriodType.Quarter;

        public string? Notes { get; set; }
    }
}
