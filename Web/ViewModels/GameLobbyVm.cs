using Domain.Entities;

namespace Web.ViewModels;

public class GameLobbyVm
{
    public string GameCode { get; set; } = string.Empty;
    public int CurrentPlayerCount { get; set; }
    public bool CanStartGame { get; set; }

    public List<GamePlayer> GamePlayers { get; set; } = [];
}
