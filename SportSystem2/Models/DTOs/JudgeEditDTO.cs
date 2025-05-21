using System.ComponentModel.DataAnnotations;

namespace SportSystem2.Models.DTOs
{
    public class JudgeEditDTO
    {
        public int JudgeId { get; set; }

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string QualificationLevel { get; set; } = null!;

        [DataType(DataType.Date)]
        public DateTime? LastAttestationDate { get; set; }

        public string ApplicationUserId { get; set; } = null!;
    }
}
