using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportSystem2.Models
{
    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public class Player
    {
        [Key]
        public int PlayerId { get; set; }

        [ForeignKey(nameof(Team))]
        public int TeamId { get; set; }

        [ValidateNever]
        public Team Team { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string Position { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Range(30, 230)]
        public int Height { get; set; }

        [Range(1, 230)]
        public int Weight { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public int Number { get; set; }

        public List<PlayerEvent> PlayerEvents { get; set; } = new();

        [Display(Name = "Photo")]
        public string? PhotoPath { get; set; }

        [NotMapped]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Year;
                if (BirthDate.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}
