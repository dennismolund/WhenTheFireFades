using Microsoft.EntityFrameworkCore;
using WhenTheFireFades.Models;

namespace WhenTheFireFades.Data.Repositories;

public class TeamProposalVoteRepository(ApplicationDbContext db) : ITeamProposalVoteRepository
{
    private readonly ApplicationDbContext _db = db;

    public async Task AddTeamProposalVoteAsync(TeamProposalVote teamProposalVote)
    {
        await _db.TeamProposalVotes.AddAsync(teamProposalVote);
    }

    public async Task<List<TeamProposalVote>> GetByTeamProposalAsync(int teamProposalId)
    {
        return await _db.TeamProposalVotes
            .Where(v => v.TeamProposalId == teamProposalId)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}