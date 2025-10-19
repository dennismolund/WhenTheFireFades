using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WhenTheFireFades.Models;

[Index(nameof(ConnectionCode), IsUnique = true)]
public class Game
{
    [Key]
    public int GameId { get; set; }

    [Required]
    [StringLength(10)]
    public string ConnectionCode { get; set; } = default!;

    [Required]
    public int LeaderSeat { get; set; }

    [Required]
    public GameStatus Status { get; set; } = GameStatus.Lobby;

    [Required]
    public GameResult GameWinner { get; set; } = GameResult.Unknown;

    [Required]
    public int RoundCounter { get; set; } = 0;

    [Required]
    public int SuccessCount { get; set; } = 0;

    [Required]
    public int SabotageCount { get; set; } = 0;

    [Required]
    public int ConsecutiveRejectedProposals { get; set; } = 0;

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<GamePlayer> Players { get; set; } = new List<GamePlayer>();
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
}
