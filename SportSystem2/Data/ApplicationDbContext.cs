using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportSystem2.Models;

namespace SportSystem2.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<Match> Matches { get; set; } = null!;
        public DbSet<TournamentRound> TournamentRounds { get; set; } = null!;
        public DbSet<PlayerEvent> PlayerEvents { get; set; } = null!;
        public DbSet<TeamStanding> TeamStandings { get; set; } = null!;
        //public DbSet<Tournament> Tournaments { get; set; } = null!;
        public DbSet<Judge> Judges { get; set; } = null!;
        //public DbSet<QualificationTest> QualificationTests { get; set; } = null!;
        public DbSet<Test> Tests { get; set; } = null!;
        public DbSet<TestResult> TestResults { get; set; } = null!;
        //public DbSet<LearningMaterial> LearningMaterials { get; set; } = null!;
        //public DbSet<SportType> SportTypes { get; set; } = null!;
        public DbSet<Team> Teams { get; set; } = null!;
        //public DbSet<PlayerStatistic> PlayerStatistics { get; set; } = null!;
        public DbSet<MatchResult> MatchResults { get; set; } = null!;
        public DbSet<GameAssignment> GameAssignments { get; set; } = null!;
        public DbSet<MatchAnalysis> MatchAnalyses { get; set; } = null!;
        public DbSet<NewsPost> NewsPosts { get; set; } = null!;
        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Match>()
                .HasOne(m => m.TeamA)
                .WithMany()
                .HasForeignKey(m => m.TeamAId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.TeamB)
                .WithMany()
                .HasForeignKey(m => m.TeamBId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GameAssignment>()
        .HasOne(ga => ga.Match)
        .WithMany(m => m.GameAssignments)
        .HasForeignKey(ga => ga.MatchId)
        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GameAssignment>()
                .HasOne(ga => ga.Judge)
                .WithMany(m => m.GameAssignments)
                .HasForeignKey(ga => ga.JudgeId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<MatchAnalysis>()
    .HasOne(ma => ma.Match)
    .WithMany(m => m.MatchAnalyses)
    .HasForeignKey(ma => ma.MatchId)
    .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Match>()
       .HasOne(m => m.Tournament)
       .WithMany(t => t.Matches)
       .HasForeignKey(m => m.TournamentId)
       .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.TournamentRound)
                .WithMany(tr => tr.Matches)
                .HasForeignKey(m => m.TournamentRoundId)
                .OnDelete(DeleteBehavior.Restrict);
         

        }

        //public DbSet<SportSystem2.Models.Test> Tests { get; set; } = default!;
        public DbSet<SportSystem2.Models.Tournament> Tournaments { get; set; } = default!;
        //public DbSet<SportSystem2.Models.TournamentRound> TournamentRounds { get; set; } = default!;
    }
}
