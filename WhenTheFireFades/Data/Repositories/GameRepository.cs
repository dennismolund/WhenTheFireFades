using Microsoft.EntityFrameworkCore;
using WhenTheFireFades.Models;

namespace WhenTheFireFades.Data.Repositories;

public class GameRepository(ApplicationDbContext db) : IGameRepository
{
    private readonly ApplicationDbContext _db = db;

    public async Task AddGameAsync(Game game)
    {
        await _db.Games.AddAsync(game);
    }

    public async Task<Game?> GetByCodeAsync(string code)
    {
        return await _db.Games
            .FirstOrDefaultAsync(g => g.ConnectionCode == code);
    }

    public async Task<Game?> GetByIdWithPlayersAndRoundsAsync(int gameId)
    {
        return await _db.Games
            .Include(g => g.Players)
            .Include(g => g.Rounds)
            .FirstOrDefaultAsync(g => g.GameId == gameId);
    }

    public async Task<Game?> GetByCodeWithPlayersAsync(string code)
    {
        var game = await _db.Games
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.ConnectionCode == code);

        return game;
    }

    public async Task<Game?> GetByIdWithPlayersAsync(int gameId)
    {
        return await _db.Games
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.GameId == gameId);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }

    public Task<Game?> GetByCodeWithPlayersAndRoundsAsync(string code)
    {
        return _db.Games
            .Include(g => g.Players)
            .Include(g => g.Rounds)
            .FirstOrDefaultAsync(g => g.ConnectionCode == code);
    }
}
