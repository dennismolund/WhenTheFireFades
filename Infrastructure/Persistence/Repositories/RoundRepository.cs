using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Persistence;

public class RoundRepository(ApplicationDbContext db) : IRoundRepository
{
    public async Task AddRoundAsync(Round round)
    {
        await db.Rounds.AddAsync(round);
    }

    public async Task<Round?> GetCurrentRoundByGameId(int gameId, int roundNumber)
    {
        Round? round = await db.Rounds.FirstOrDefaultAsync(r => r.GameId == gameId && r.RoundNumber == roundNumber);
        return round;
    }

    public async Task UpdateRoundStatus(int roundId, RoundStatus status)
    {
        var round = await db.Rounds.FindAsync(roundId);
        if (round != null)
        {
            round.Status = status;
            round.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }

    public async Task<Round?> GetCurrentRoundSnapshot(int gameId, int roundNumber)
    {
        return await db.Rounds
            .Include(r => r.Teams.Where(tp => tp.IsActive))
                .ThenInclude(tp => tp.Votes)        
            .Include(r => r.Teams.Where(tp => tp.IsActive))
                .ThenInclude(tp => tp.Members)   
            .FirstOrDefaultAsync(r => r.GameId == gameId && r.RoundNumber == roundNumber);
    }

}
