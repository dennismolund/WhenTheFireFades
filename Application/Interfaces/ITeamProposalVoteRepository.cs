using Domain.Entities;

namespace Application.Interfaces;

public interface ITeamProposalVoteRepository
{
    Task AddTeamProposalVoteAsync(TeamProposalVote teamProposalVote);
    Task<List<TeamProposalVote>> GetByTeamProposalAsync(int teamProposalId);
    Task SaveChangesAsync();
}
