using WhenTheFireFades.Models;

namespace WhenTheFireFades.Data.Repositories;

public interface ITeamProposalRepository
{
    Task AddTeamProposalAsync(TeamProposal teamProposal);
    Task<TeamProposal?> GetByIdAsync(int teamProposalId);
    Task<TeamProposal?> GetByIdWithVotesAsync(int teamProposalId);
    Task<TeamProposal?> GetByRoundIdAsync(int roundId);
    Task<TeamProposal?> GetActiveByRoundIdAsync(int roundId);
    Task SaveChangesAsync();
}
