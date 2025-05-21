using System.ComponentModel.DataAnnotations;

namespace SportSystem2.Models.DTOs
{
    public class JudgeCreateDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string QualificationLevel { get; set; } = null!;
    }
}
