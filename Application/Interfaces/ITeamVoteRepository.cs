using Domain.Entities;

namespace Application.Interfaces;

public interface ITeamVoteRepository
{
    Task AddTeamVoteAsync(TeamVote teamVote);
    Task<List<TeamVote>> GetByTeamAsync(int teamId);
    Task SaveChangesAsync();
}
