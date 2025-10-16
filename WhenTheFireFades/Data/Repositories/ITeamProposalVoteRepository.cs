using WhenTheFireFades.Models;

namespace WhenTheFireFades.Data.Repositories;

public interface ITeamProposalVoteRepository
{
    Task AddTeamProposalVoteAsync(TeamProposalVote teamProposalVote);
    Task<List<TeamProposalVote>> GetByTeamProposalAsync(int teamProposalId);
    Task SaveChangesAsync();
}
