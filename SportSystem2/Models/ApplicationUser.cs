using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SportSystem2.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? PhotoUrl { get; set; }
        public bool? IsJudge { get; set; } = false;
        public Judge? Judge { get; set; }

        public List<NewsPost> NewsPosts { get; set; } = new();
    }
}
