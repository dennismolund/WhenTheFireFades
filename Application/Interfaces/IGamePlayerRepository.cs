using Domain.Entities;

namespace Application.Interfaces;

public interface IGamePlayerRepository
{
    Task AddPlayerAsync(GamePlayer player);
    void RemovePlayer(GamePlayer player);
    Task<int> GetNextAvailableSeatAsync(int gameId);
    Task SaveChangesAsync();
}
