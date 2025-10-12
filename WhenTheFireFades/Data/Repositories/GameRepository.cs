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

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }

    public async Task<Game?> GetByCodeWithPlayersAsync(string code)
    {
        var game = await _db.Games
            .Include(g => g.Players)
            .FirstOrDefaultAsync(g => g.ConnectionCode == code);

        return game;
    }

}
