using Domain.Entities;

namespace Web.ViewModels;

public class LobbyViewModel
{
    public string ConnectionCode { get; set; } = string.Empty;
    public int PlayerCount { get; set; }
    public GamePlayer CurrentPlayer { get; set; } = default!;
    public Game Game { get; set; } = default!;
    public List<GamePlayer> GamePlayers { get; set; } = [];
    public bool IsLeader => CurrentPlayer.Seat == Game.LeaderSeat;
}
