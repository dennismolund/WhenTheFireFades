using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TeamProposalVoteRepository(ApplicationDbContext db) : ITeamProposalVoteRepository
{
    public async Task AddTeamProposalVoteAsync(TeamProposalVote teamProposalVote)
    {
        await db.TeamProposalVotes.AddAsync(teamProposalVote);
    }

    public async Task<List<TeamProposalVote>> GetByTeamProposalAsync(int teamProposalId)
    {
        return await db.TeamProposalVotes
            .Where(v => v.TeamProposalId == teamProposalId)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }
}