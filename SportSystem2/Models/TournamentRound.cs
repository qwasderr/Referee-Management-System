using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportSystem2.Models
{
    public class TournamentRound
    {
        [Key]
        public int RoundId { get; set; }

        [Required]
        public int TournamentId { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(TournamentId))]
        public Tournament Tournament { get; set; } = null!;

        [Required]
        public string RoundName { get; set; } = null!;

        [Required]
        public string Location { get; set; } = null!;

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        public List<Match> Matches { get; set; } = new();
    }
}
