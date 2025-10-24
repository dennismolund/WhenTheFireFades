using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Persistence;

public class TeamProposalRepository(ApplicationDbContext db) : ITeamProposalRepository
{
    public async Task AddTeamProposalAsync(TeamProposal teamProposal)
    {
        await db.TeamProposals.AddAsync(teamProposal);
    }

    public async Task<TeamProposal?> GetByIdAsync(int teamProposalId)
    {
        return await db.TeamProposals.FindAsync(teamProposalId);
    }

    public async Task<TeamProposal?> GetByIdWithVotesAsync(int teamProposalId)
    {
        return await db.TeamProposals
            .Include(tp => tp.Votes)
            .FirstOrDefaultAsync(tp => tp.TeamProposalId == teamProposalId);
    }

    public async Task<TeamProposal?> GetByRoundIdAsync(int roundId)
    {
        return await db.TeamProposals
           .Include(tp => tp.Members)
           .Include(tp => tp.Votes)
           .OrderByDescending(tp => tp.AttemptNumber)
           .FirstOrDefaultAsync(tp => tp.RoundId == roundId);
    }

    public async Task<TeamProposal?> GetActiveByRoundIdAsync(int roundId)
    {
        return await db.TeamProposals
       .Where(tp => tp.RoundId == roundId && tp.IsActive)
       .Include(tp => tp.Votes)
       .Include(tp => tp.Members)
       .OrderByDescending(tp => tp.AttemptNumber)
       .FirstOrDefaultAsync();
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }
}
