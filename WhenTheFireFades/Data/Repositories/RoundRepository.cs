using Microsoft.EntityFrameworkCore;
using WhenTheFireFades.Models;

namespace WhenTheFireFades.Data.Repositories;

public class RoundRepository(ApplicationDbContext db) : IRoundRepository
{
    private readonly ApplicationDbContext _db = db;

    public async Task AddRoundAsync(Round round)
    {
        await _db.Rounds.AddAsync(round);
    }

    public async Task<Round?> GetCurrentRoundByGameId(int gameId, int roundNumber)
    {
        Round? round = await _db.Rounds.FirstOrDefaultAsync(r => r.GameId == gameId && r.RoundNumber == roundNumber);
        return round;
    }

    public async Task UpdateRoundStatus(int roundId, RoundStatus status)
    {
        var round = await _db.Rounds.FindAsync(roundId);
        if (round != null)
        {
            round.Status = status;
            round.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }

    public async Task<Round?> GetCurrentRoundSnapshot(int gameId, int roundNumber)
    {
        return await _db.Rounds
            .Include(r => r.TeamProposals.Where(tp => tp.IsActive))
                .ThenInclude(tp => tp.Votes)        
            .Include(r => r.TeamProposals.Where(tp => tp.IsActive))
                .ThenInclude(tp => tp.Members)   
            .FirstOrDefaultAsync(r => r.GameId == gameId && r.RoundNumber == roundNumber);
    }
}
