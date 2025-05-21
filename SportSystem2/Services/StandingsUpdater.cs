using Microsoft.EntityFrameworkCore;
using SportSystem2.Data;
using SportSystem2.Models;

namespace SportSystem2.Services
{
    public class StandingsUpdater : IStandingsUpdater
    {
        private readonly ApplicationDbContext _context;

        public StandingsUpdater(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task UpdateTeamStandingsAsync(int tournamentId)
        {
            var matchResults = await _context.MatchResults
                .Where(mr => mr.Match.TournamentId == tournamentId)
                .Include(mr => mr.Team)
                .Include(mr => mr.Match)
                .ToListAsync();

            var standings = matchResults
                .GroupBy(mr => mr.TeamId)
                .Select(g =>
                {
                    var team = g.First().Team;
                    var matches = g.ToList();
                    int wins = 0, draws = 0, losses = 0, totalPoints = 0, conceded = 0;

                    foreach (var result in matches)
                    {
                        var opponent = matchResults.FirstOrDefault(r =>
                            r.MatchId == result.MatchId && r.TeamId != result.TeamId);

                        if (opponent != null)
                        {
                            conceded += opponent.Points;

                            if (result.Points > opponent.Points)
                                (wins, totalPoints) = (wins + 1, totalPoints + 3);
                            else if (result.Points == opponent.Points)
                                (draws, totalPoints) = (draws + 1, totalPoints + 2);
                            else
                                (losses, totalPoints) = (losses + 1, totalPoints + 1);
                        }
                    }

                    return new TeamStanding
                    {
                        TeamId = team.TeamId,
                        TournamentId = tournamentId,
                        Wins = wins,
                        Draws = draws,
                        Losses = losses,
                        Points = totalPoints,
                        Scored = matches.Sum(m => m.Points),
                        Conceded = conceded
                    };
                })
                .OrderByDescending(ts => ts.Points)
                .ThenByDescending(ts => ts.Wins)
                .ToList();

            for (int i = 0; i < standings.Count; i++)
                standings[i].Position = i + 1;

            var oldStandings = _context.TeamStandings
                .Where(s => s.TournamentId == tournamentId);
            _context.TeamStandings.RemoveRange(oldStandings);

            await _context.TeamStandings.AddRangeAsync(standings);
            await _context.SaveChangesAsync();
        }
    }
}
