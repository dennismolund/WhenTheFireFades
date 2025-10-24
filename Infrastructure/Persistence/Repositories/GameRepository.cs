using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class GameRepository(ApplicationDbContext db) : IGameRepository
{
    public async Task AddGameAsync(Game game)
    {
        await db.Games.AddAsync(game);
    }

    public async Task<Game?> GetByCodeAsync(string code)
    {
        return await db.Games
            .FirstOrDefaultAsync(g => g.ConnectionCode == code);
    }

    public async Task<Game?> GetByIdWithPlayersAndRoundsAsync(int gameId)
    {
        return await db.Games
            .Include(g => g.Players)
            .Include(g => g.Rounds)
            .FirstOrDefaultAsync(g => g.GameId == gameId);
    }

    public async Task<Game?> GetByCodeWithPlayersAsync(string code)
    {
        var game = await db.Games
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.ConnectionCode == code);

        return game;
    }

    public async Task<Game?> GetByIdWithPlayersAsync(int gameId)
    {
        return await db.Games
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.GameId == gameId);
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }

    public Task<Game?> GetByCodeWithPlayersAndRoundsAsync(string code)
    {
        return db.Games
            .Include(g => g.Players)
            .Include(g => g.Rounds)
            .FirstOrDefaultAsync(g => g.ConnectionCode == code);
    }
}
