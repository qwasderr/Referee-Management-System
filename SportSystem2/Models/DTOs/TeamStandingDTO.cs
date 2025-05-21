using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SportSystem2.Models.DTO
{
    public class TeamStandingDTO
    {
        public int Position { get; set; }
        public string TeamName { get; set; }
        public string TeamPhotoUrl { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int Points { get; set; }
        public int Scored { get; set; }
        public int Conceded { get; set; }
        public int Difference => Scored - Conceded;
    }
}

