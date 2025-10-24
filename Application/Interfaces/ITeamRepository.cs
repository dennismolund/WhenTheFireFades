using Domain.Entities;

namespace Application.Interfaces;

public interface ITeamRepository
{
    Task AddTeamAsync(Team team);
    Task<Team?> GetByIdAsync(int teamId);
    Task<Team?> GetByIdWithVotesAsync(int teamId);
    Task<Team?> GetByRoundIdAsync(int roundId);
    Task<Team?> GetActiveByRoundIdAsync(int roundId);
    Task SaveChangesAsync();
}
