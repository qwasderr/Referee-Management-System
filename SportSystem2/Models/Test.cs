using System.ComponentModel.DataAnnotations;

namespace SportSystem2.Models
{
    public class Test
    {
        [Key]
        public int TestId { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string TestUrl { get; set; } = null!;

        public List<TestResult> TestResults { get; set; } = new();
    }
}
