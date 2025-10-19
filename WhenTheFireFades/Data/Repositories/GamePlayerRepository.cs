using Microsoft.EntityFrameworkCore;
using WhenTheFireFades.Models;

namespace WhenTheFireFades.Data.Repositories;

public class GamePlayerRepository(ApplicationDbContext db) : IGamePlayerRepository
{
    private readonly ApplicationDbContext _db = db;

    public async Task AddPlayerAsync(GamePlayer player)
    {
        await _db.GamePlayers
            .AddAsync(player);
    }

    public async Task<GamePlayer?> GetByGameAndTempUserAsync(int gameId, int tempUserId)
    {
        return await _db.GamePlayers
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

        var takenSeats = await _db.GamePlayers
            .Where(gp => gp.GameId == gameId)
            .OrderBy(gp => gp.Seat)
            .Select(gp => gp.Seat)
            .ToListAsync();

        var expectedSeat = 1;

        foreach (var seat in takenSeats)
        {
            if (seat > expectedSeat)
            {
                break;
            }

            if (seat == expectedSeat)
            {
                expectedSeat++;
            }
        }

        return expectedSeat;
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

    public void RemovePlayer(GamePlayer player)
    {
        _db.GamePlayers.Remove(player);
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
