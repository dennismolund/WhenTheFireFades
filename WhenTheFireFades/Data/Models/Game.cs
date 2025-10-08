using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WhenTheFireFades.Data.Models;

public partial class Game
{
    [Key]
    public int GameId { get; set; }

    [StringLength(10)]
    public string ConnectionCode { get; set; } = null!;

    public int LeaderSeatId { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [StringLength(100)]
    public string? GameWinner { get; set; }

    public int RoundCounter { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    [InverseProperty("Game")]
    public virtual ICollection<GamePlayer> GamePlayers { get; set; } = [];

    [InverseProperty("Game")]
    public virtual ICollection<Round> Rounds { get; set; } = [];
}
