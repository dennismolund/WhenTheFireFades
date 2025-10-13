using WhenTheFireFades.Models;

namespace WhenTheFireFades.Data.Repositories;

public interface IGameRepository
{
    Task<Game?> GetByCodeAsync(string code);
    Task<Game?> GetByCodeWithPlayersAsync(string code);
    Task<Game?> GetByIdWithPlayersAsync(int gameId);
    Task<Game?> GetByIdWithPlayersAndRoundsAsync(int gameId);
    Task<Game?> GetByCodeWithPlayersAndRoundsAsync(string code);
    Task AddGameAsync(Game game);
    Task SaveChangesAsync();
}
