using WhenTheFireFades.Models;

namespace WhenTheFireFades.Data.Repositories;

public interface IMissionVoteRepository
{

    Task AddMissionVoteAsync(MissionVote missionVote);

    Task<List<MissionVote>> GetByRoundIdAsync(int roundId);
    Task SaveChangesAsync();
}
