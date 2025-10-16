using WhenTheFireFades.Models;

namespace WhenTheFireFades.Data.Repositories;

public interface IRoundRepository
{
    Task AddRoundAsync(Round round);
    Task<Round?> GetCurrentRoundByGameId(int gameId, int roundNumber);
    Task UpdateTeamVoteCounter(int roundId);
    Task SaveChangesAsync();
    Task UpdateRoundStatus(int roundId, RoundStatus status);
}
