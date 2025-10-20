using Domain.Entities;

namespace Application.Interfaces;

public interface IGamePlayerRepository
{
    Task<GamePlayer?> GetByIdAsync(int gamePlayerId);
    Task<GamePlayer?> GetByGameAndTempUserAsync(int gameId, int tempUserId);
    Task<GamePlayer?> GetByGameCodeAndTempUserAsync(string gameCode, int tempUserId);
    Task<List<GamePlayer>> GetPlayersByGameIdAsync(int gameId);
    Task<List<GamePlayer>> GetPlayersByGameCodeAsync(string gameCode);
    Task<bool> IsPlayerInGameAsync(int gameId, int tempUserId);
    Task<int> GetPlayerCountAsync(int gameId);
    Task AddPlayerAsync(GamePlayer player);
    Task UpdatePlayerAsync(GamePlayer player);
    void RemovePlayer(GamePlayer player);
    Task<int> GetNextAvailableSeatAsync(int gameId);
    Task<bool> IsSeatTakenAsync(int gameId, int seat);
    Task SaveChangesAsync();
}
