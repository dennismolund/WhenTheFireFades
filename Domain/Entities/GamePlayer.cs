using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class GamePlayer
{
    [Key]
    public int GamePlayerId { get; set; }

    [Required]
    public int GameId { get; set; }

    public int? TempUserId { get; set; }
    
    public string? UserId { get; set; }

    [StringLength(40)]
    public string Nickname { get; set; } = string.Empty;

    [Required]
    public int Seat { get; set; }

    [Required]
    public PlayerRole Role { get; set; } = PlayerRole.Human;

    [Required]
    public bool IsReady { get; set; } = false;

    [Required]
    public bool IsConnected { get; set; } = true;

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(GameId))]
    public Game Game { get; set; } = default!;
}

