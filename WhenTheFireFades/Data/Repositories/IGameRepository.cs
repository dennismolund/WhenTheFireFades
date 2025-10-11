using WhenTheFireFades.Models;

namespace WhenTheFireFades.Data.Repositories;

public interface IGameRepository
{
    Task AddGameAsync(Game game);
    Task SaveChangesAsync();
    Task<Game?> GetByCodeAsync(string code);

}
