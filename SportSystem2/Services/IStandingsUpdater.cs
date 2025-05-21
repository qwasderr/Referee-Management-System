namespace SportSystem2.Services
{
    public interface IStandingsUpdater
    {
        Task UpdateTeamStandingsAsync(int tournamentId);
    }

}
