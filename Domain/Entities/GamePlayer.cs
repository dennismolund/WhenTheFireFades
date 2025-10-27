using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class GamePlayer
{
    [Key]
    public int GamePlayerId { get; set; }

    [Required]
    public int GameId { get; set; }

    public int? TempUserId { get; set; }
    
    public string? UserId { get; set; }

    [MaxLength(40)]
    public string Nickname { get; set; } = string.Empty;

    [Required]
    public int Seat { get; set; }

    [Required]
    public PlayerRole Role { get; set; } = PlayerRole.Human;
    
    [Required]
    public bool IsConnected { get; set; } = true;
    
    public Game Game { get; set; } = default!;
    
    
}

