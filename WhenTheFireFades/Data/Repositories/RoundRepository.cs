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

    public async Task<Round> GetCurrentRoundByGameId(int gameId, int roundNumber)
    {
        Round round = await _db.Rounds.FirstOrDefaultAsync(r => r.GameId == gameId && r.RoundNumber == roundNumber);
        return round;
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
