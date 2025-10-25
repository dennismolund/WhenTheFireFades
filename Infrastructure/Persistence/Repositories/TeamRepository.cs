using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TeamRepository(ApplicationDbContext db) : ITeamRepository
{
    public async Task AddTeamAsync(Team team)
    {
        await db.Teams.AddAsync(team);
    }

    public async Task<Team?> GetByIdAsync(int teamId)
    {
        return await db.Teams.FindAsync(teamId);
    }

    public async Task<Team?> GetByIdWithVotesAsync(int teamId)
    {
        return await db.Teams
            .Include(tp => tp.Votes)
            .FirstOrDefaultAsync(tp => tp.TeamId == teamId);
    }

    public async Task<Team?> GetByRoundIdAsync(int roundId)
    {
        return await db.Teams
           .Include(tp => tp.Members)
           .Include(tp => tp.Votes)
           .FirstOrDefaultAsync(tp => tp.RoundId == roundId);
    }

    public async Task<Team?> GetActiveByRoundIdAsync(int roundId)
    {
        return await db.Teams
       .Where(tp => tp.RoundId == roundId && tp.IsActive)
       .Include(tp => tp.Votes)
       .Include(tp => tp.Members)
       .FirstOrDefaultAsync();
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }
}
