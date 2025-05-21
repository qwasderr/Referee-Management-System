using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportSystem2.Models
{
    public class NewsPost
    {
        [Key]
        public int NewsPostId { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        [Display(Name = "Photo")]
        public string? PhotoPath { get; set; }
    }
}
