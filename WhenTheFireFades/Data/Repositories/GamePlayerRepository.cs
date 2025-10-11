using Microsoft.EntityFrameworkCore;
using WhenTheFireFades.Models;

namespace WhenTheFireFades.Data.Repositories;

public class GamePlayerRepository(ApplicationDbContext db) : IGamePlayerRepository
{
    private readonly ApplicationDbContext _db = db;

    //NOTE: Jag kanske kommer behöva ändra AddPlayerAsync till att returnera den tillagda spelaren med dess genererade ID
    public async Task AddPlayerAsync(GamePlayer player)
    {
        await _db.GamePlayers
            .AddAsync(player);
    }

    public async Task<GamePlayer?> GetByGameAndTempUserAsync(int gameId, int tempUserId)
    {
        return await _db.GamePlayers
            .AsNoTracking() // Detta verkar göra att entiteten inte spåras av DbContext vilket kan vara bra för read-only operationer då dem kan bli snabbare: https://learn.microsoft.com/en-us/ef/core/querying/tracking
            .SingleOrDefaultAsync(p => p.GameId == gameId && p.TempUserId == tempUserId);
    }

    public Task<GamePlayer?> GetByGameCodeAndTempUserAsync(string gameCode, int tempUserId)
    {
        throw new NotImplementedException();
    }

    public Task<GamePlayer?> GetByIdAsync(int gamePlayerId)
    {
        throw new NotImplementedException();
    }

    public async Task<int> GetNextAvailableSeatAsync(int gameId)
    {
        var nextSeat = await _db.GamePlayers
            .Where(gp => gp.GameId == gameId).ToListAsync();

        return nextSeat.Count + 1;
    }

    public Task<int> GetPlayerCountAsync(int gameId)
    {
        throw new NotImplementedException();
    }

    public Task<List<GamePlayer>> GetPlayersByGameCodeAsync(string gameCode)
    {
        throw new NotImplementedException();
    }

    public Task<List<GamePlayer>> GetPlayersByGameIdAsync(int gameId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsPlayerInGameAsync(int gameId, int tempUserId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsSeatTakenAsync(int gameId, int seat)
    {
        throw new NotImplementedException();
    }

    public Task RemovePlayerAsync(GamePlayer player)
    {
        throw new NotImplementedException();
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }

    public Task UpdatePlayerAsync(GamePlayer player)
    {
        throw new NotImplementedException();
    }
}
