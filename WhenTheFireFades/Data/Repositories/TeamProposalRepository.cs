using Microsoft.EntityFrameworkCore;
using WhenTheFireFades.Models;

namespace WhenTheFireFades.Data.Repositories;

public class TeamProposalRepository(ApplicationDbContext db) : ITeamProposalRepository
{
    private readonly ApplicationDbContext _db = db;

    public async Task AddTeamProposalAsync(TeamProposal teamProposal)
    {
        await _db.TeamProposals.AddAsync(teamProposal);
    }

    public async Task<TeamProposal?> GetByIdAsync(int teamProposalId)
    {
        return await _db.TeamProposals.FindAsync(teamProposalId);
    }

    public async Task<TeamProposal?> GetByIdWithVotesAsync(int teamProposalId)
    {
        return await _db.TeamProposals
            .Include(tp => tp.Votes)
            .FirstOrDefaultAsync(tp => tp.TeamProposalId == teamProposalId);
    }

    public async Task<TeamProposal?> GetByRoundIdAsync(int roundId)
    {
        return await _db.TeamProposals
           .Include(tp => tp.Members)
           .Include(tp => tp.Votes)
           .OrderByDescending(tp => tp.AttemptNumber)
           .FirstOrDefaultAsync(tp => tp.RoundId == roundId);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
