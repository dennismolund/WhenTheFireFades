using Domain.Entities;

namespace Application.Interfaces;

public interface IMissionVoteRepository
{

    Task AddMissionVoteAsync(MissionVote missionVote);
    Task<List<MissionVote>> GetByRoundIdAsync(int roundId);
    Task SaveChangesAsync();
}
